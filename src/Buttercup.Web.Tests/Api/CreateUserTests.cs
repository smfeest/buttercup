using Buttercup.Application;
using Buttercup.Web.TestUtils;
using HotChocolate;
using Xunit;

namespace Buttercup.Web.Api;

public sealed class CreateUserTests(AppFactory appFactory) : EndToEndTests(appFactory)
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task CreatingUser(bool isAdmin)
    {
        var currentUser = this.ModelFactory.BuildUser() with { IsAdmin = true };
        await this.DatabaseFixture.InsertEntities(currentUser);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);

        var attributes = this.BuildNewUserAttributes() with { IsAdmin = isAdmin };

        using var response = await PostCreateUserMutation(client, attributes);
        using var document = await response.Content.ReadAsJsonDocument();

        var createUserElement = ApiAssert.SuccessResponse(document).GetProperty("createUser");
        var userElement = createUserElement.GetProperty("user");
        var id = userElement.GetProperty("id").GetInt64();

        var expected = new
        {
            id,
            attributes.Name,
            attributes.Email,
            attributes.TimeZone,
            isAdmin,
            Revision = 0
        };
        JsonAssert.Equivalent(expected, userElement);

        JsonAssert.ValueIsNull(createUserElement.GetProperty("errors"));
    }

    [Fact]
    public async Task CreatingUserWhenNotAnAdmin()
    {
        var currentUser = this.ModelFactory.BuildUser() with { IsAdmin = false };
        await this.DatabaseFixture.InsertEntities(currentUser);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);

        var attributes = this.BuildNewUserAttributes();

        using var response = await PostCreateUserMutation(client, attributes);
        using var document = await response.Content.ReadAsJsonDocument();

        JsonAssert.ValueIsNull(document.RootElement.GetProperty("data"));
        ApiAssert.HasSingleError(ErrorCodes.Authentication.NotAuthorized, document);
    }

    [Fact]
    public async Task CreatingUserWithInvalidAttributes()
    {
        var currentUser = this.ModelFactory.BuildUser() with { IsAdmin = true };
        await this.DatabaseFixture.InsertEntities(currentUser);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);

        var attributes = new NewUserAttributes
        {
            Name = new('a', 251),
            Email = "example.com",
            TimeZone = "",
        };

        using var response = await PostCreateUserMutation(client, attributes);
        using var document = await response.Content.ReadAsJsonDocument();

        var createUserElement = ApiAssert.SuccessResponse(document).GetProperty("createUser");

        JsonAssert.ValueIsNull(createUserElement.GetProperty("user"));

        var expectedErrors = new[]
        {
            new
            {
                Message = "This field is limited to 250 characters",
                Path = new string[] { "input", "attributes", "name" },
                Code = "INVALID_STRING_LENGTH",
            },
            new
            {
                Message = "This field must contain a valid email address",
                Path = new string[] { "input", "attributes", "email" },
                Code = "INVALID_FORMAT",
            },
            new
            {
                Message = "This field is required",
                Path = new string[] { "input", "attributes", "timeZone" },
                Code = "REQUIRED",
            },
        };
        JsonAssert.Equivalent(expectedErrors, createUserElement.GetProperty("errors"));
    }

    private NewUserAttributes BuildNewUserAttributes() => new()
    {
        Name = this.ModelFactory.NextString("name"),
        Email = this.ModelFactory.NextEmail(),
        TimeZone = "Europe/Prague",
        IsAdmin = true,
    };

    private static Task<HttpResponseMessage> PostCreateUserMutation(
        HttpClient client, NewUserAttributes attributes) =>
        client.PostQuery("""
            mutation($attributes: NewUserAttributesInput!) {
                createUser(input: { attributes: $attributes }) {
                    user {
                        id
                        name
                        email
                        timeZone
                        isAdmin
                        revision
                    }
                    errors {
                        ... on ValidationError {
                            message
                            path
                            code
                        }
                    }
                }
            }
            """,
            new { attributes });
}
