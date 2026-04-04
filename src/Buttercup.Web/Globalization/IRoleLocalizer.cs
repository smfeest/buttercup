using Buttercup.EntityModel;

namespace Buttercup.Web.Globalization;

/// <summary>
/// Defines the contract for the role localizer.
/// </summary>
public interface IRoleLocalizer
{
    /// <summary>
    /// Gets the localized name for a role.
    /// </summary>
    /// <param name="role">
    /// The role to localize.
    /// </param>
    /// <returns>
    /// The localized name for the role.
    /// </returns>
    string this[Role role] { get; }
}
