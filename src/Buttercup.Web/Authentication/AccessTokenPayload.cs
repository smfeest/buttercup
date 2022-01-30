namespace Buttercup.Web.Authentication;

/// <summary>
/// Represents an access token's payload.
/// </summary>
/// <param name="UserId">
/// The user ID.
/// </param>
/// <param name="SecurityStamp">
/// The user's security stamp at the time of issue.
/// </param>
/// <param name="Issued">
/// The time of issue.
/// </param>
public sealed record AccessTokenPayload(long UserId, string SecurityStamp, DateTime Issued);
