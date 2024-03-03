using System.Security.Claims;
using Buttercup.Application;
using Buttercup.Security;
using HotChocolate.Authorization;

namespace Buttercup.Web.Api;

[MutationType]
public sealed class Mutation
{
    public async Task<AuthenticatePayload> Authenticate(
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] IPasswordAuthenticationService passwordAuthenticationService,
        [Service] ITokenAuthenticationService tokenAuthenticationService,
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

        return new(true, accessToken, user);
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
}
