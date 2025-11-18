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
}
