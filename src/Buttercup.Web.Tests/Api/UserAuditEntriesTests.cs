using System.Net;
using Buttercup.EntityModel;
using Buttercup.Web.TestUtils;
using HotChocolate;
using Xunit;

namespace Buttercup.Web.Api;

public sealed class UserAuditEntriesTests(AppFactory appFactory) : EndToEndTests(appFactory)
{
    private const string UserAuditEntriesQuery = """
        query {
            userAuditEntries {
                nodes {
                    id
                    time
                    operation
                    target { id name }
                    actor { id name }
                    ipAddress
                }
            }
        }
        """;

    [Fact]
    public async Task QueryingUserAuditEntries()
    {
        var currentUser = this.ModelFactory.BuildUser(true) with { IsAdmin = true };
        var otherUser = this.ModelFactory.BuildUser(true);
        var baseTime = this.ModelFactory.NextDateTime();
        var userAuditEntries = new UserAuditEntry[]
        {
            new()
            {
                Id = 1,
                Time = baseTime,
                Operation = UserAuditOperation.Create,
                Target = currentUser,
                Actor = otherUser,
                IpAddress = IPAddress.Parse("10.20.30.40"),
            },
            new()
            {
                Id = 2,
                Time = baseTime.AddHours(1),
                Operation = UserAuditOperation.ChangePassword,
                Target = otherUser,
                Actor = currentUser,
                IpAddress = null,
            },
        };
        await this.DatabaseFixture.InsertEntities(currentUser, userAuditEntries);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);
        using var response = await client.PostQuery(UserAuditEntriesQuery);
        using var document = await response.Content.ReadAsJsonDocument();

        var dataElement = ApiAssert.SuccessResponse(document);
        JsonAssert.Equivalent(
            new[]
            {
                new
                {
                    Id = 1,
                    Time = baseTime,
                    Operation = "CREATE",
                    Target = IdName.From(currentUser),
                    Actor = IdName.From(otherUser),
                    IpAddress = (string?)"10.20.30.40",
                },
                new
                {
                    Id = 2,
                    Time = baseTime.AddHours(1),
                    Operation = "CHANGE_PASSWORD",
                    Target = IdName.From(otherUser),
                    Actor = IdName.From(currentUser),
                    IpAddress = (string?)null,
                },
            },
            dataElement.GetProperty("userAuditEntries").GetProperty("nodes"));
    }

    [Fact]
    public async Task QueryingUserAuditEntriesWhenNotAnAdmin()
    {
        var currentUser = this.ModelFactory.BuildUser() with { IsAdmin = false };
        await this.DatabaseFixture.InsertEntities(currentUser);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);
        using var response = await client.PostQuery(UserAuditEntriesQuery);
        using var document = await response.Content.ReadAsJsonDocument();

        JsonAssert.ValueIsNull(
            document.RootElement.GetProperty("data").GetProperty("userAuditEntries"));
        ApiAssert.HasSingleError(ErrorCodes.Authentication.NotAuthorized, document);
    }

    [Fact]
    public async Task FilteringUserAuditEntries()
    {
        var currentUser = this.ModelFactory.BuildUser(true) with { IsAdmin = true };
        var otherUser = this.ModelFactory.BuildUser() with { Email = "other@example.com" };
        var userAuditEntries = new UserAuditEntry[]
        {
            new()
            {
                Id = 1,
                Operation = UserAuditOperation.Create,
                Target = currentUser,
                Actor = currentUser,
                IpAddress = IPAddress.Parse("10.20.30.40"),
            },
            new()
            {
                Id = 2,
                Operation = UserAuditOperation.Create,
                Target = otherUser,
                Actor = currentUser,
                IpAddress = IPAddress.Parse("10.20.30.40"),
            },
            new()
            {
                Id = 3,
                Operation = UserAuditOperation.Create,
                Target = otherUser,
                Actor = currentUser,
                IpAddress = IPAddress.Parse("192.168.0.2"),
            },
            new()
            {
                Id = 4,
                Operation = UserAuditOperation.Create,
                Target = otherUser,
                Actor = currentUser,
                IpAddress = IPAddress.Parse("10.20.30.40"),
            },
        };
        await this.DatabaseFixture.InsertEntities(currentUser, otherUser, userAuditEntries);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);
        using var response = await client.PostQuery("""
            query {
                userAuditEntries(
                    where: {
                        and: [
                            { ipAddress: { eq: "10.20.30.40" } },
                            { target: { email: { eq: "other@example.com" } } }
                        ]
                    }
                ) { nodes { id } }
            }
            """);
        using var document = await response.Content.ReadAsJsonDocument();

        var dataElement = ApiAssert.SuccessResponse(document);

        var actualOrderedIds = dataElement
            .GetProperty("userAuditEntries")
            .GetProperty("nodes")
            .EnumerateArray()
            .Select(e => e.GetProperty("id").GetInt64());

        Assert.Equal([2, 4], actualOrderedIds);
    }

    [Fact]
    public async Task SortingUserAuditEntries()
    {
        var currentUser = this.ModelFactory.BuildUser(true) with { IsAdmin = true };
        var baseTime = this.ModelFactory.NextDateTime();
        var userAuditEntries = new UserAuditEntry[]
        {
            new()
            {
                Id = 1,
                Time = baseTime,
                Operation = UserAuditOperation.Create,
                Target = currentUser,
                Actor = currentUser,
            },
            new()
            {
                Id = 2,
                Time = baseTime.AddMinutes(2),
                Operation = UserAuditOperation.Create,
                Target = currentUser,
                Actor = currentUser,
            },
            new()
            {
                Id = 3,
                Time = baseTime.AddMinutes(1),
                Operation = UserAuditOperation.Create,
                Target = currentUser,
                Actor = currentUser,
            },
        };
        await this.DatabaseFixture.InsertEntities(currentUser, userAuditEntries);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);
        using var response = await client.PostQuery("""
            query {
                userAuditEntries(order: { time: DESC }) {
                    nodes { id }
                }
            }
            """);
        using var document = await response.Content.ReadAsJsonDocument();

        var dataElement = ApiAssert.SuccessResponse(document);

        var actualOrderedIds = dataElement
            .GetProperty("userAuditEntries")
            .GetProperty("nodes")
            .EnumerateArray()
            .Select(e => e.GetProperty("id").GetInt64());

        Assert.Equal([2, 3, 1], actualOrderedIds);
    }
}
