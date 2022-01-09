namespace Buttercup.Models;

/// <summary>
/// Represents a user.
/// </summary>
/// <param name="Id">
/// The user ID.
/// </param>
/// <param name="Name">
/// The name.
/// </param>
/// <param name="Email">
/// The email address.
/// </param>
/// <param name="HashedPassword">
/// The hashed password.
/// </param>
/// <param name="PasswordCreated">
/// The date and time at which the user last changed their password.
/// </param>
/// <param name="SecurityStamp">
/// The security stamp; an opaque string that changes whenever the user's existing
/// sessions need to be invalidate.
/// </param>
/// <param name="TimeZone">
/// The TZ ID of the user's time zone.
/// </param>
/// <param name="Created">
/// The date and time at which the record was created.
/// </param>
/// <param name="Modified">
/// The date and time at which the record was last modified.
/// </param>
/// <param name="Revision">
/// The revision number.
/// </param>
public sealed record User(
    long Id,
    string? Name,
    string? Email,
    string? HashedPassword,
    DateTime? PasswordCreated,
    string? SecurityStamp,
    string? TimeZone,
    DateTime Created,
    DateTime Modified,
    int Revision);
