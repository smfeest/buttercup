using Buttercup.Application;
using Buttercup.Web.TestUtils;
using HotChocolate;
using Xunit;

namespace Buttercup.Web.Api;

public sealed class AccessTokenInvalidationTests(AppFactory appFactory) : EndToEndTests(appFactory)
{
    [Fact]
    public async Task QueryingApiFollowingDeactivation()
    {
        var user = this.ModelFactory.BuildUser() with { IsAdmin = true };
        await this.DatabaseFixture.InsertEntities(user);

        using var client = await this.AppFactory.CreateClientForApiUser(user);

        await this.AppFactory.Services.GetRequiredService<IUserManager>().DeactivateUser(
            user.Id, user.Id, null);

        using var response = await client.PostQuery("query { recipes { id } }");
        using var document = await response.Content.ReadAsJsonDocument();

        JsonAssert.ValueIsNull(document.RootElement.GetProperty("data"));
        ApiAssert.HasSingleError(ErrorCodes.Authentication.NotAuthenticated, document);
    }
}
