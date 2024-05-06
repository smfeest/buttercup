using Buttercup.Web.TestUtils;
using Xunit;

namespace Buttercup.Web.Api;

public sealed class CurrentUserTests(AppFactory<CurrentUserTests> appFactory)
    : EndToEndTests<CurrentUserTests>(appFactory)
{
    private const string CurrentUserQuery =
        @"query {
            currentUser {
                id
                name
                email
                passwordCreated
                timeZone
                created
                modified
            }
        }";

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task QueryingCurrentUser(bool setOptionalAttributes)
    {
        var user = this.ModelFactory.BuildUser(setOptionalAttributes);
        await this.DatabaseFixture.InsertEntities(user);

        using var client = await this.AppFactory.CreateClientForApiUser(user);
        using var response = await client.PostQuery(CurrentUserQuery);
        using var document = await response.Content.ReadAsJsonDocument();

        var dataElement = ApiAssert.SuccessResponse(document);

        var expected = new
        {
            user.Id,
            user.Name,
            user.Email,
            user.PasswordCreated,
            user.TimeZone,
            user.Created,
            user.Modified,
        };

        JsonAssert.Equivalent(expected, dataElement.GetProperty("currentUser"));
    }

    [Fact]
    public async Task QueryingCurrentUserWhenUnauthenticated()
    {
        using var client = this.AppFactory.CreateClient();
        using var response = await client.PostQuery(CurrentUserQuery);
        using var document = await response.Content.ReadAsJsonDocument();

        var dataElement = ApiAssert.SuccessResponse(document);

        JsonAssert.ValueIsNull(dataElement.GetProperty("currentUser"));
    }
}
