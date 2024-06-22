using System.Security.Claims;
using Buttercup.Application;
using Buttercup.EntityModel;
using Buttercup.Security;
using Buttercup.Web.Security;
using HotChocolate.Authorization;
using HotChocolate.Execution;
using HotChocolate.Resolvers;
using Microsoft.Extensions.Localization;
using IAuthorizationService = Microsoft.AspNetCore.Authorization.IAuthorizationService;

namespace Buttercup.Web.Api;

[MutationType]
public sealed class Mutation
{
    public async Task<AuthenticatePayload> Authenticate(
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] IPasswordAuthenticationService passwordAuthenticationService,
        [Service] ITokenAuthenticationService tokenAuthenticationService,
        [Service] IClaimsIdentityFactory claimsIdentityFactory,
        ClaimsPrincipal claimsPrincipal,
        string email,
        string password)
    {
        var ipAddress = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress;

        var user = await passwordAuthenticationService.Authenticate(email, password, ipAddress);

        if (user == null)
        {
            return new(false);
        }

        var accessToken = await tokenAuthenticationService.IssueAccessToken(user, ipAddress);

        claimsPrincipal.AddIdentity(claimsIdentityFactory.CreateIdentityForUser(user));

        return new(true, accessToken, user);
    }

    /// <summary>
    /// Creates a comment on a recipe.
    /// </summary>
    /// <param name="validatorFactory">
    /// The input object validator factory.
    /// </param>
    /// <param name="commentManager">
    /// The comment manager.
    /// </param>
    /// <param name="claimsPrincipal">
    /// The claims principal.
    /// </param>
    /// <param name="schema">
    /// The GraphQL schema.
    /// </param>
    /// <param name="recipeId">
    /// The recipe ID.
    /// </param>
    /// <param name="attributes">
    /// The comment attributes.
    /// </param>
    [Authorize]
    [Error<NotFoundException>]
    [Error<InputObjectValidationError>]
    [Error<SoftDeletedException>]
    public async Task<MutationResult<CreateCommentPayload>> CreateComment(
        [Service] IInputObjectValidatorFactory validatorFactory,
        [Service] ICommentManager commentManager,
        ClaimsPrincipal claimsPrincipal,
        ISchema schema,
        long recipeId,
        CommentAttributes attributes)
    {
        var validator = validatorFactory.CreateValidator<CommentAttributes>(schema);
        var validationErrors = new List<InputObjectValidationError>();

        if (!validator.Validate(attributes, ["input", "attributes"], validationErrors))
        {
            return new(validationErrors);
        }

        var id = await commentManager.AddComment(recipeId, attributes, claimsPrincipal.GetUserId());
        return new CreateCommentPayload(id);
    }

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
    public async Task<MutationResult<CreateRecipePayload, InputObjectValidationError>> CreateRecipe(
        [Service] IInputObjectValidatorFactory validatorFactory,
        [Service] IRecipeManager recipeManager,
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

        var id = await recipeManager.AddRecipe(attributes, claimsPrincipal.GetUserId());
        return new CreateRecipePayload(id);
    }

    /// <summary>
    /// Soft-deletes a comment.
    /// </summary>
    /// <param name="authorizationService">
    /// The authorization service.
    /// </param>
    /// <param name="commentManager">
    /// The comment manager.
    /// </param>
    /// <param name="localizer">
    /// The string localizer.
    /// </param>
    /// <param name="dbContext">
    /// The database context.
    /// </param>
    /// <param name="claimsPrincipal">
    /// The claims principal.
    /// </param>
    /// <param name="resolverContext">
    /// The resolver context.
    /// </param>
    /// <param name="id">
    /// The comment ID.
    /// </param>
    public async Task<DeleteCommentPayload> DeleteComment(
        [Service] IAuthorizationService authorizationService,
        [Service] ICommentManager commentManager,
        [Service] IStringLocalizer<Mutation> localizer,
        AppDbContext dbContext,
        ClaimsPrincipal claimsPrincipal,
        IResolverContext resolverContext,
        long id)
    {
        var comment = await dbContext.Comments.FindAsync(id);

        var authorizationResult = await authorizationService.AuthorizeAsync(
            claimsPrincipal, comment, AuthorizationPolicyNames.CommentAuthorOrAdmin);

        return authorizationResult.Succeeded
            ? new(id, await commentManager.DeleteComment(id, claimsPrincipal.GetUserId()))
            : throw new QueryException(
                resolverContext.CreateError(
                    ErrorCodes.Authentication.NotAuthorized,
                    localizer["Error_DeleteCommentNotAuthorized"]));
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
        [Service] IRecipeManager recipeManager, ClaimsPrincipal claimsPrincipal, long id) =>
        new(id, await recipeManager.DeleteRecipe(id, claimsPrincipal.GetUserId()));

    /// <summary>
    /// Hard-deletes a comment.
    /// </summary>
    /// <param name="commentManager">
    /// The comment manager.
    /// </param>
    /// <param name="id">
    /// The comment ID.
    /// </param>
    [Authorize(Policy = AuthorizationPolicyNames.AdminOnly)]
    public async Task<HardDeleteCommentPayload> HardDeleteComment(
        [Service] ICommentManager commentManager, long id) =>
        new(await commentManager.HardDeleteComment(id));

    /// <summary>
    /// Hard-deletes a recipe.
    /// </summary>
    /// <param name="recipeManager">
    /// The recipe manager.
    /// </param>
    /// <param name="id">
    /// The recipe ID.
    /// </param>
    [Authorize(Policy = AuthorizationPolicyNames.AdminOnly)]
    public async Task<HardDeleteRecipePayload> HardDeleteRecipe(
        [Service] IRecipeManager recipeManager, long id) =>
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
    public async Task<MutationResult<UpdateRecipePayload>> UpdateRecipe(
        [Service] IInputObjectValidatorFactory validatorFactory,
        [Service] IRecipeManager recipeManager,
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
