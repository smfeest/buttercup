namespace Buttercup.Web.Api;

/// <param name="Deleted">
/// <b>true</b> if the recipe was deleted; <b>false</b> if no recipe was found with the
/// specified ID.
/// </param>
public sealed record DeleteRecipePayload(bool Deleted);
