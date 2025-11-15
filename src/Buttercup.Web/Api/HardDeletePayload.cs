namespace Buttercup.Web.Api;

/// <param name="Deleted">
/// <b>true</b> if the record was hard-deleted; <b>false</b> if no record was found with the
/// specified ID.
/// </param>
public sealed record HardDeletePayload(bool Deleted);
