using Buttercup.EntityModel;
using Buttercup.Web.TestUtils;
using HotChocolate;
using Xunit;

namespace Buttercup.Web.Api;

public sealed class UsersTests(AppFactory appFactory) : EndToEndTests(appFactory)
{
    private const string UsersQuery = """
        query {
            users {
                id
                name
                email
                passwordCreated
                timeZone
                created
                modified
                revision
            }
        }
        """;

    [Fact]
    public async Task QueryingUsers()
    {
        var currentUser = this.ModelFactory.BuildUser(true) with { IsAdmin = true };
        var otherUser = this.ModelFactory.BuildUser(false);
        await this.DatabaseFixture.InsertEntities(currentUser, otherUser);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);
        using var response = await client.PostQuery(UsersQuery);
        using var document = await response.Content.ReadAsJsonDocument();

        var dataElement = ApiAssert.SuccessResponse(document);

        var expected = new User[] { currentUser, otherUser }.Select(user => new
        {
            user.Id,
            user.Name,
            user.Email,
            user.PasswordCreated,
            user.TimeZone,
            user.Created,
            user.Modified,
            user.Revision,
        });

        JsonAssert.Equivalent(expected, dataElement.GetProperty("users"));
    }

    [Fact]
    public async Task QueryingUsersWhenUnauthenticated()
    {
        using var client = this.AppFactory.CreateClient();
        using var response = await client.PostQuery(UsersQuery);
        using var document = await response.Content.ReadAsJsonDocument();

        JsonAssert.ValueIsNull(document.RootElement.GetProperty("data"));
        ApiAssert.HasSingleError(ErrorCodes.Authentication.NotAuthenticated, document);
    }

    [Fact]
    public async Task FilteringUsersByUnrestrictedFieldsWhenNotAdmin()
    {
        var users = new[]
        {
            this.ModelFactory.BuildUser() with { Id = 1, Name = "Anna", IsAdmin = false },
            this.ModelFactory.BuildUser() with { Id = 2, Name = "Ben" },
            this.ModelFactory.BuildUser() with { Id = 3, Name = "Adam" },
        };
        await this.DatabaseFixture.InsertEntities(users);

        using var client = await this.AppFactory.CreateClientForApiUser(users[0]);
        using var response = await client.PostQuery("""
            query {
                users(
                    order: { id: ASC },
                    where: { name: { startsWith: "A" } }
                ) { id }
            }
            """);
        using var document = await response.Content.ReadAsJsonDocument();

        var dataElement = ApiAssert.SuccessResponse(document);
        var filteredIds = dataElement.GetProperty("users").EnumerateArray().Select(
            u => u.GetProperty("id").GetInt64());

        Assert.Equal([1, 3], filteredIds);
    }

    [Fact]
    public async Task FilteringUsersByEmailWhenNotAnAdmin()
    {
        var currentUser = this.ModelFactory.BuildUser() with { IsAdmin = false };
        await this.DatabaseFixture.InsertEntities(currentUser);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);
        using var response = await client.PostQuery("""
            query {
                users(where: { email: { contains: "a" } }) { id }
            }
            """);
        using var document = await response.Content.ReadAsJsonDocument();

        JsonAssert.ValueIsNull(document.RootElement.GetProperty("data"));
        ApiAssert.HasSingleError(ErrorCodes.Authentication.NotAuthorized, document);
    }

    [Fact]
    public async Task SortingUsersByUnrestrictedFieldsWhenNotAnAdmin()
    {
        var users = new[]
        {
            this.ModelFactory.BuildUser() with { Id = 1, Name = "Anna", IsAdmin = false },
            this.ModelFactory.BuildUser() with { Id = 2, Name = "Chris" },
            this.ModelFactory.BuildUser() with { Id = 3, Name = "Ben" },
            this.ModelFactory.BuildUser() with { Id = 4, Name = "Anna" },
        };
        await this.DatabaseFixture.InsertEntities(users);

        using var client = await this.AppFactory.CreateClientForApiUser(users[0]);
        using var response = await client.PostQuery("""
            query {
                users(order: [{ name: ASC, id: DESC }]) { id name }
            }
            """);
        using var document = await response.Content.ReadAsJsonDocument();

        var dataElement = ApiAssert.SuccessResponse(document);

        Assert.Collection(
            dataElement.GetProperty("users").EnumerateArray(),
            u => JsonAssert.Equivalent(new { Id = 4, Name = "Anna" }, u),
            u => JsonAssert.Equivalent(new { Id = 1, Name = "Anna" }, u),
            u => JsonAssert.Equivalent(new { Id = 3, Name = "Ben" }, u),
            u => JsonAssert.Equivalent(new { Id = 2, Name = "Chris" }, u));
    }

    [Fact]
    public async Task SortingUsersByEmailWhenAnAdmin()
    {
        var users = new[]
        {
            this.ModelFactory.BuildUser() with
            {
                Id = 1,
                Email = "craig@example.com",
                IsAdmin = true
            },
            this.ModelFactory.BuildUser() with { Id = 2, Email = "mary@example.com" },
            this.ModelFactory.BuildUser() with { Id = 3, Email = "adam@example.com" },
        };
        await this.DatabaseFixture.InsertEntities(users);

        using var client = await this.AppFactory.CreateClientForApiUser(users[0]);
        using var response = await client.PostQuery("""
            query {
                users(order: [{ email: ASC }]) { id }
            }
            """);
        using var document = await response.Content.ReadAsJsonDocument();

        var dataElement = ApiAssert.SuccessResponse(document);
        var sortedIds = dataElement.GetProperty("users").EnumerateArray().Select(
            u => u.GetProperty("id").GetInt64());

        Assert.Equal([3, 1, 2], sortedIds);
    }

    [Fact]
    public async Task SortingUsersByEmailWhenNotAnAdmin()
    {
        var currentUser = this.ModelFactory.BuildUser() with { IsAdmin = false };
        await this.DatabaseFixture.InsertEntities(currentUser);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);
        using var response = await client.PostQuery("""
            query {
                users(order: [{ name: ASC, email: DESC }]) { id }
            }
            """);
        using var document = await response.Content.ReadAsJsonDocument();

        JsonAssert.ValueIsNull(document.RootElement.GetProperty("data"));
        ApiAssert.HasSingleError(ErrorCodes.Authentication.NotAuthorized, document);
    }
}
