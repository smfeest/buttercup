using Buttercup.DataAccess;
using Buttercup.Models;

namespace Buttercup.Web.Api;

[ExtendObjectType<User>(
    IgnoreProperties = new[] { nameof(User.HashedPassword), nameof(User.SecurityStamp) })]
public class UserExtension
{
    [DataLoader]
    public static async Task<IReadOnlyDictionary<long, User>> GetUsersByIdAsync(
        IReadOnlyList<long> keys,
        IMySqlConnectionSource mySqlConnectionSource,
        IUserDataProvider userDataProvider)
    {
        using var connection = await mySqlConnectionSource.OpenConnection();

        var users = await userDataProvider.GetUsers(connection, keys);

        return users.ToDictionary(x => x.Id);
    }
}
