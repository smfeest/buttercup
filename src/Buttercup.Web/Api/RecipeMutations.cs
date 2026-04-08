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
    /// <param name="cancellationToken">
    /// The cancellation token.
    /// </param>
    [Authorize]
    public async Task<FieldResult<CreateRecipePayload, InputObjectValidationError>> CreateRecipe(
        IInputObjectValidatorFactory validatorFactory,
        IRecipeManager recipeManager,
        ClaimsPrincipal claimsPrincipal,
        ISchema schema,
        RecipeAttributes attributes,
        CancellationToken cancellationToken)
    {
        var validator = validatorFactory.CreateValidator<RecipeAttributes>(schema);
        var validationErrors = new List<InputObjectValidationError>();

        if (!validator.Validate(attributes, ["input", "attributes"], validationErrors))
        {
            return new(validationErrors);
        }

        var id = await recipeManager.CreateRecipe(
            attributes, claimsPrincipal.GetUserId(), cancellationToken);
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
    /// <param name="cancellationToken">
    /// The cancellation token.
    /// </param>
    [Authorize]
    public async Task<DeleteRecipePayload> DeleteRecipe(
        IRecipeManager recipeManager,
        ClaimsPrincipal claimsPrincipal,
        long id,
        CancellationToken cancellationToken) =>
        new(
            id,
            await recipeManager.DeleteRecipe(id, claimsPrincipal.GetUserId(), cancellationToken));

    /// <summary>
    /// Hard-deletes a recipe.
    /// </summary>
    /// <param name="recipeManager">
    /// The recipe manager.
    /// </param>
    /// <param name="id">
    /// The recipe ID.
    /// </param>
    /// <param name="cancellationToken">
    /// The cancellation token.
    /// </param>
    [Authorize(AuthorizationPolicyNames.AdminOnly)]
    [UseMutationConvention(PayloadTypeName = nameof(HardDeletePayload))]
    public async Task<HardDeletePayload> HardDeleteRecipe(
        IRecipeManager recipeManager, long id, CancellationToken cancellationToken) =>
        new(await recipeManager.HardDeleteRecipe(id, cancellationToken));

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
    /// <param name="cancellationToken">
    /// The cancellation token.
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
        int baseRevision,
        CancellationToken cancellationToken)
    {
        var validator = validatorFactory.CreateValidator<RecipeAttributes>(schema);
        var validationErrors = new List<InputObjectValidationError>();

        if (!validator.Validate(attributes, ["input", "attributes"], validationErrors))
        {
            return new(validationErrors);
        }

        await recipeManager.UpdateRecipe(
            id, attributes, baseRevision, claimsPrincipal.GetUserId(), cancellationToken);
        return new UpdateRecipePayload(id);
    }
}
