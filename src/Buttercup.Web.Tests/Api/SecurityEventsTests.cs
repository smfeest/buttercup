using System.Net;
using Buttercup.Web.TestUtils;
using HotChocolate;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Buttercup.Web.Api;

public sealed class SecurityEventsTests(AppFactory appFactory) : EndToEndTests(appFactory)
{
    private const string SecurityEventsQuery = """
        query {
            securityEvents {
                nodes {
                    id
                    time
                    event
                    user { id name }
                    ipAddress
                }
            }
        }
        """;

    [Fact]
    public async Task QueryingSecurityEvents()
    {
        var currentUser = this.ModelFactory.BuildUser(true) with { IsAdmin = true };
        await this.DatabaseFixture.InsertEntities(currentUser);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);

        using var dbContext = this.AppFactory.DatabaseFixture.CreateDbContext();
        await dbContext.SecurityEvents.ExecuteDeleteAsync(TestContext.Current.CancellationToken);

        var securityEvents = new[]
        {
            this.ModelFactory.BuildSecurityEvent(setOptionalAttributes: true),
            this.ModelFactory.BuildSecurityEvent(
                this.ModelFactory.BuildUser(), setOptionalAttributes: false)
        };
        await this.DatabaseFixture.InsertEntities(securityEvents);

        using var response = await client.PostQuery(SecurityEventsQuery);
        using var document = await response.Content.ReadAsJsonDocument();

        var dataElement = ApiAssert.SuccessResponse(document);

        var expected = securityEvents.Select(securityEvent => new
        {
            securityEvent.Id,
            securityEvent.Time,
            securityEvent.Event,
            User = IdName.From(securityEvent.User),
            IpAddress = securityEvent.IpAddress?.ToString(),
        });

        JsonAssert.Equivalent(
            expected, dataElement.GetProperty("securityEvents").GetProperty("nodes"));
    }

    [Fact]
    public async Task QueryingSecurityEventsWhenNotAnAdmin()
    {
        var currentUser = this.ModelFactory.BuildUser() with { IsAdmin = false };
        await this.DatabaseFixture.InsertEntities(currentUser);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);
        using var response = await client.PostQuery(SecurityEventsQuery);
        using var document = await response.Content.ReadAsJsonDocument();

        JsonAssert.ValueIsNull(
            document.RootElement.GetProperty("data").GetProperty("securityEvents"));
        ApiAssert.HasSingleError(ErrorCodes.Authentication.NotAuthorized, document);
    }

    [Fact]
    public async Task FilteringSecurityEvents()
    {
        var currentUser = this.ModelFactory.BuildUser(true) with { IsAdmin = true };
        var dorothy = this.ModelFactory.BuildUser() with { Email = "dorthy@example.com" };
        var otherUser = this.ModelFactory.BuildUser();
        var securityEvents = new[]
        {
            this.ModelFactory.BuildSecurityEvent(otherUser) with
            {
                Id = 1,
                IpAddress = IPAddress.Parse("10.20.30.40"),
            },
            this.ModelFactory.BuildSecurityEvent(dorothy) with
            {
                Id = 2,
                IpAddress = IPAddress.Parse("10.20.30.40"),
            },
            this.ModelFactory.BuildSecurityEvent(dorothy) with
            {
                Id = 3,
                IpAddress = IPAddress.Parse("192.168.0.2"),
            },
            this.ModelFactory.BuildSecurityEvent(dorothy) with
            {
                Id = 4,
                IpAddress = IPAddress.Parse("10.20.30.40"),
            },
        };
        await this.DatabaseFixture.InsertEntities(currentUser, securityEvents);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);
        using var response = await client.PostQuery("""
            query {
                securityEvents(
                    where: {
                        and: [
                            { ipAddress: { eq: "10.20.30.40" } },
                            { user: { email: { eq: "dorthy@example.com" } } }
                        ]
                    }
                ) { nodes { id } }
            }
            """);
        using var document = await response.Content.ReadAsJsonDocument();

        var dataElement = ApiAssert.SuccessResponse(document);

        var actualOrderedIds = dataElement
            .GetProperty("securityEvents")
            .GetProperty("nodes")
            .EnumerateArray()
            .Select(e => e.GetProperty("id").GetInt64());

        Assert.Equal([2, 4], actualOrderedIds);
    }

    [Fact]
    public async Task SortingSecurityEvents()
    {
        var currentUser = this.ModelFactory.BuildUser(true) with { IsAdmin = true };
        await this.DatabaseFixture.InsertEntities(currentUser);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);

        using var dbContext = this.AppFactory.DatabaseFixture.CreateDbContext();
        await dbContext.SecurityEvents.ExecuteDeleteAsync(TestContext.Current.CancellationToken);

        var securityEvents = new[]
        {
            this.ModelFactory.BuildSecurityEvent() with { Id = 1 },
            this.ModelFactory.BuildSecurityEvent() with { Id = 3 },
            this.ModelFactory.BuildSecurityEvent() with { Id = 2 },
        };
        await this.DatabaseFixture.InsertEntities(securityEvents);

        using var response = await client.PostQuery("""
            query {
                securityEvents(order: { id: DESC }) {
                    nodes { id }
                }
            }
            """);
        using var document = await response.Content.ReadAsJsonDocument();

        var dataElement = ApiAssert.SuccessResponse(document);

        var actualOrderedIds = dataElement
            .GetProperty("securityEvents")
            .GetProperty("nodes")
            .EnumerateArray()
            .Select(e => e.GetProperty("id").GetInt64());

        Assert.Equal([3, 2, 1], actualOrderedIds);
    }
}
