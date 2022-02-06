using System.Security.Claims;
using Buttercup.DataAccess;
using Buttercup.Models;
using Buttercup.Web.Authentication;

namespace Buttercup.Web.Api;

public sealed class Query
{
    private readonly IMySqlConnectionSource mySqlConnectionSource;

    public Query(IMySqlConnectionSource mySqlConnectionSource) =>
        this.mySqlConnectionSource = mySqlConnectionSource;

    public async Task<User?> CurrentUser(
        [Service] IUserDataProvider userDataProvider, ClaimsPrincipal principal)
    {
        var userId = principal.GetUserId();

        if (!userId.HasValue)
        {
            return null;
        }

        using var connection = await this.mySqlConnectionSource.OpenConnection();

        return await userDataProvider.GetUser(connection, userId.Value);
    }
}
