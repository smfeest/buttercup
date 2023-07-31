namespace Buttercup.Security;

/// <summary>
/// Provides constants for custom claim types.
/// </summary>
public static class CustomClaimTypes
{
    /// <summary>
    /// The URI for a claim that specifies the security stamp for an entity.
    /// </summary>
    public const string SecurityStamp = "http://schemas.smf.me.uk/buttercup/security-stamp";

    /// <summary>
    /// The URI for a claim that specifies the time zone for an entity.
    /// </summary>
    public const string TimeZone = "http://schemas.smf.me.uk/buttercup/time-zone";

    /// <summary>
    /// The URI for a claim that specifies the user revision.
    /// </summary>
    public const string UserRevision = "http://schemas.smf.me.uk/buttercup/user-revision";
}
