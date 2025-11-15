using System.Security.Claims;
using Buttercup.Application;
using Buttercup.EntityModel;
using Buttercup.Security;
using Buttercup.Web.Security;
using HotChocolate.Authorization;
using HotChocolate.Resolvers;
using Microsoft.Extensions.Localization;
using IAuthorizationService = Microsoft.AspNetCore.Authorization.IAuthorizationService;

namespace Buttercup.Web.Api;

[MutationType]
public sealed class CommentMutations
{
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
    public async Task<FieldResult<CreateCommentPayload>> CreateComment(
        IInputObjectValidatorFactory validatorFactory,
        ICommentManager commentManager,
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

        var id = await commentManager.CreateComment(recipeId, attributes, claimsPrincipal.GetUserId());
        return new CreateCommentPayload(id);
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
    [Authorize]
    public async Task<DeleteCommentPayload> DeleteComment(
        IAuthorizationService authorizationService,
        ICommentManager commentManager,
        IStringLocalizer<CommentMutations> localizer,
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
            : throw new GraphQLException(
                resolverContext.CreateError(
                    ErrorCodes.Authentication.NotAuthorized,
                    localizer["Error_DeleteCommentNotAuthorized"]));
    }

    /// <summary>
    /// Hard-deletes a comment.
    /// </summary>
    /// <param name="commentManager">
    /// The comment manager.
    /// </param>
    /// <param name="id">
    /// The comment ID.
    /// </param>
    [Authorize(AuthorizationPolicyNames.AdminOnly)]
    [UseMutationConvention(PayloadTypeName = nameof(HardDeletePayload))]
    public async Task<HardDeletePayload> HardDeleteComment(
        ICommentManager commentManager, long id) =>
        new(await commentManager.HardDeleteComment(id));
}
