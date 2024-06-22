namespace Buttercup.Web.Api;

/// <param name="Deleted">
/// <b>true</b> if the comment was hard-deleted; <b>false</b> if no comment was found with the
/// specified ID.
/// </param>
public sealed record HardDeleteCommentPayload(bool Deleted);
