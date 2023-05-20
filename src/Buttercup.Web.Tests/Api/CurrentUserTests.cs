using Buttercup.Web.TestUtils;
using Xunit;

namespace Buttercup.Web.Api;

public class CurrentUserTests : EndToEndTests<CurrentUserTests>
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

    public CurrentUserTests(AppFactory<CurrentUserTests> appFactory) : base(appFactory)
    {
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task QueryingCurrentUser(bool setOptionalAttributes)
    {
        var user = this.ModelFactory.BuildUser(setOptionalAttributes);

        await this.DatabaseFixture.InsertEntities(user);

        using var client = await this.AppFactory.CreateClientForApiUser(user);

        var response = await client.PostQuery(CurrentUserQuery);

        var dataElement = await ApiAssert.SuccessResponse(response);

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

        JsonAssert.ValueEquals(expected, dataElement.GetProperty("currentUser"));
    }

    [Fact]
    public async Task QueryingCurrentUserWhenUnauthenticated()
    {
        using var client = this.AppFactory.CreateClient();

        var response = await client.PostQuery(CurrentUserQuery);

        var dataElement = await ApiAssert.SuccessResponse(response);

        JsonAssert.ValueIsNull(dataElement.GetProperty("currentUser"));
    }
}
