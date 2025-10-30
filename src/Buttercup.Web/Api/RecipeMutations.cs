using System.Security.Claims;
using Buttercup.Application;
using Buttercup.EntityModel;
using Buttercup.Security;
using Buttercup.Web.Security;
using HotChocolate.Authorization;

namespace Buttercup.Web.Api;

[MutationType]
public sealed class RecipeMutations
{
    /// <summary>
    /// Creates a recipe.
    /// </summary>
    /// <param name="validatorFactory">
    /// The input object validator factory.
    /// </param>
    /// <param name="recipeManager">
    /// The recipe manager.
    /// </param>
    /// <param name="claimsPrincipal">
    /// The claims principal.
    /// </param>
    /// <param name="schema">
    /// The GraphQL schema.
    /// </param>
    /// <param name="attributes">
    /// The recipe attributes.
    /// </param>
    [Authorize]
    public async Task<FieldResult<CreateRecipePayload, InputObjectValidationError>> CreateRecipe(
        IInputObjectValidatorFactory validatorFactory,
        IRecipeManager recipeManager,
        ClaimsPrincipal claimsPrincipal,
        ISchema schema,
        RecipeAttributes attributes)
    {
        var validator = validatorFactory.CreateValidator<RecipeAttributes>(schema);
        var validationErrors = new List<InputObjectValidationError>();

        if (!validator.Validate(attributes, ["input", "attributes"], validationErrors))
        {
            return new(validationErrors);
        }

        var id = await recipeManager.CreateRecipe(attributes, claimsPrincipal.GetUserId());
        return new CreateRecipePayload(id);
    }

    /// <summary>
    /// Soft-deletes a recipe.
    /// </summary>
    /// <param name="recipeManager">
    /// The recipe manager.
    /// </param>
    /// <param name="claimsPrincipal">
    /// The claims principal.
    /// </param>
    /// <param name="id">
    /// The recipe ID.
    /// </param>
    [Authorize]
    public async Task<DeleteRecipePayload> DeleteRecipe(
        IRecipeManager recipeManager, ClaimsPrincipal claimsPrincipal, long id) =>
        new(id, await recipeManager.DeleteRecipe(id, claimsPrincipal.GetUserId()));

    /// <summary>
    /// Hard-deletes a recipe.
    /// </summary>
    /// <param name="recipeManager">
    /// The recipe manager.
    /// </param>
    /// <param name="id">
    /// The recipe ID.
    /// </param>
    [Authorize(AuthorizationPolicyNames.AdminOnly)]
    public async Task<HardDeleteRecipePayload> HardDeleteRecipe(
        IRecipeManager recipeManager, long id) =>
        new(await recipeManager.HardDeleteRecipe(id));

    /// <summary>
    /// Updates a recipe.
    /// </summary>
    /// <param name="validatorFactory">
    /// The input object validator factory.
    /// </param>
    /// <param name="recipeManager">
    /// The recipe manager.
    /// </param>
    /// <param name="claimsPrincipal">
    /// The claims principal.
    /// </param>
    /// <param name="schema">
    /// The GraphQL schema.
    /// </param>
    /// <param name="id">
    /// The recipe ID.
    /// </param>
    /// <param name="attributes">
    /// The recipe attributes.
    /// </param>
    /// <param name="baseRevision">
    /// The base revision. Used for concurrency control.
    /// </param>
    [Authorize]
    [Error<ConcurrencyException>]
    [Error<NotFoundException>]
    [Error<InputObjectValidationError>]
    [Error<SoftDeletedException>]
    public async Task<FieldResult<UpdateRecipePayload>> UpdateRecipe(
        IInputObjectValidatorFactory validatorFactory,
        IRecipeManager recipeManager,
        ClaimsPrincipal claimsPrincipal,
        ISchema schema,
        long id,
        RecipeAttributes attributes,
        int baseRevision)
    {
        var validator = validatorFactory.CreateValidator<RecipeAttributes>(schema);
        var validationErrors = new List<InputObjectValidationError>();

        if (!validator.Validate(attributes, ["input", "attributes"], validationErrors))
        {
            return new(validationErrors);
        }

        await recipeManager.UpdateRecipe(id, attributes, baseRevision, claimsPrincipal.GetUserId());
        return new UpdateRecipePayload(id);
    }
}
