using System.Net;
using Buttercup.EntityModel;
using Buttercup.Web.TestUtils;
using HotChocolate;
using Xunit;

namespace Buttercup.Web.Api;

public sealed class CommentAuditsTests(AppFactory appFactory) : EndToEndTests(appFactory)
{
    private const string CommentAuditsQuery = """
        query {
            commentAudits {
                nodes {
                    id
                    comment { id body }
                    time
                    action
                    revision { id body }
                    actor { id name }
                    ipAddress
                }
            }
        }
        """;

    [Fact]
    public async Task QueryingCommentAudits()
    {
        var currentUser = this.ModelFactory.BuildUser() with { IsAdmin = true };
        var audits = new[]
        {
            this.ModelFactory.BuildCommentAudit(
                this.ModelFactory.BuildComment(setRecipe: true),
                CommentAction.Create,
                setOptionalAttributes: true),
            this.ModelFactory.BuildCommentAudit(
                this.ModelFactory.BuildComment(setRecipe: true, softDeleted: true),
                CommentAction.Delete),
        };
        await this.DatabaseFixture.InsertEntities(currentUser, audits);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);
        using var response = await client.PostQuery(CommentAuditsQuery);
        using var document = await response.Content.ReadAsJsonDocument();

        var dataElement = ApiAssert.SuccessResponse(document);

        var expected = audits.Select(audit => new
        {
            audit.Id,
            Comment = new { audit.Comment!.Id, audit.Comment.Body },
            audit.Time,
            Action = audit.Action.ToString().ToUpperInvariant(),
            Revision = audit.Revision is null ?
                null :
                new { audit.Revision.Id, audit.Revision.Body },
            Actor = IdName.From(audit.Actor),
            IpAddress = audit.IpAddress?.ToString(),
        });

        JsonAssert.Equivalent(
            expected, dataElement.GetProperty("commentAudits").GetProperty("nodes"));
    }

    [Fact]
    public async Task QueryingCommentAuditsWhenNotAnAdmin()
    {
        var currentUser = this.ModelFactory.BuildUser() with { IsAdmin = false };
        await this.DatabaseFixture.InsertEntities(currentUser);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);
        using var response = await client.PostQuery(CommentAuditsQuery);
        using var document = await response.Content.ReadAsJsonDocument();

        JsonAssert.ValueIsNull(
            document.RootElement.GetProperty("data").GetProperty("commentAudits"));
        ApiAssert.HasSingleError(ErrorCodes.Authentication.NotAuthorized, document);
    }

    [Fact]
    public async Task FilteringCommentAudits()
    {
        var currentUser = this.ModelFactory.BuildUser(true) with { IsAdmin = true };

        var comment = this.ModelFactory.BuildComment(setRecipe: true);
        var actor = this.ModelFactory.BuildUser();

        var audit = this.ModelFactory.BuildCommentAudit(comment, CommentAction.Create) with
        {
            Actor = actor,
        };
        var auditOtherComment = this.ModelFactory.BuildCommentAudit(
            this.ModelFactory.BuildComment(setRecipe: true), CommentAction.Create) with
        {
            Actor = actor,
        };
        var auditOtherActor = this.ModelFactory.BuildCommentAudit(comment, CommentAction.Create) with
        {
            Actor = this.ModelFactory.BuildUser(),
        };

        await this.DatabaseFixture.InsertEntities(
            currentUser, audit, auditOtherActor, auditOtherComment);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);
        using var response = await client.PostQuery("""
            query($commentId: Long!, $actorId: Long!) {
                commentAudits(
                    where: {
                        and: [
                            { comment: { id: { eq: $commentId } } }
                            { actor: { id: { eq: $actorId } } }
                        ]
                    }
                ) { nodes { id } }
            }
            """,
            new { commentId = comment.Id, actorId = actor.Id });
        using var document = await response.Content.ReadAsJsonDocument();

        var dataElement = ApiAssert.SuccessResponse(document);

        var returnedIds = dataElement
            .GetProperty("commentAudits")
            .GetProperty("nodes")
            .EnumerateArray()
            .Select(e => e.GetProperty("id").GetInt64());

        Assert.Equal(audit.Id, Assert.Single(returnedIds));
    }

    [Fact]
    public async Task SortingCommentAudits()
    {
        var currentUser = this.ModelFactory.BuildUser() with { IsAdmin = true };

        var comment = this.ModelFactory.BuildComment(setRecipe: true);

        CommentAudit AuditWithIpAddress(string ipAddress) => this.ModelFactory.BuildCommentAudit(
            comment, CommentAction.Create) with
        {
            IpAddress = IPAddress.Parse(ipAddress),
        };

        var auditA = AuditWithIpAddress("::aa");
        var auditC = AuditWithIpAddress("10.10.10.5");
        var auditD = AuditWithIpAddress("10.10.10.1");
        var auditB = AuditWithIpAddress("::ff");

        await this.DatabaseFixture.InsertEntities(currentUser, auditA, auditC, auditD, auditB);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);

        using var response = await client.PostQuery("""
            query {
                commentAudits(order: { ipAddress: ASC }) {
                    nodes { id }
                }
            }
            """);
        using var document = await response.Content.ReadAsJsonDocument();

        var dataElement = ApiAssert.SuccessResponse(document);

        var returnedIds = dataElement
            .GetProperty("commentAudits")
            .GetProperty("nodes")
            .EnumerateArray()
            .Select(e => e.GetProperty("id").GetInt64());

        Assert.Equal([auditA.Id, auditB.Id, auditD.Id, auditC.Id], returnedIds);
    }
}
