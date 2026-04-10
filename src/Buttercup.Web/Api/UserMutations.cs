using System.Security.Claims;
using Buttercup.Application;
using Buttercup.EntityModel;
using Buttercup.Security;
using Buttercup.Web.Security;
using HotChocolate.Authorization;
using Microsoft.Extensions.Localization;

namespace Buttercup.Web.Api;

[MutationType]
public sealed class UserMutations
{
    /// <summary>
    /// Creates a user.
    /// </summary>
    /// <param name="claimsPrincipal">
    /// The claims principal.
    /// </param>
    /// <param name="httpContextAccessor">
    /// The HTTP context accessor.
    /// </param>
    /// <param name="validatorFactory">
    /// The input object validator factory.
    /// </param>
    /// <param name="userManager">
    /// The user manager.
    /// </param>
    /// <param name="schema">
    /// The GraphQL schema.
    /// </param>
    /// <param name="localizer">
    /// The string localizer.
    /// </param>
    /// <param name="attributes">
    /// The new user attributes.
    /// </param>
    /// <param name="cancellationToken">
    /// The cancellation token.
    /// </param>
    [Authorize(AuthorizationPolicyNames.AdminOnly)]
    public async Task<FieldResult<CreateUserPayload, InputObjectValidationError>> CreateUser(
        ClaimsPrincipal claimsPrincipal,
        IHttpContextAccessor httpContextAccessor,
        IInputObjectValidatorFactory validatorFactory,
        IUserManager userManager,
        ISchema schema,
        IStringLocalizer<UserMutations> localizer,
        NewUserAttributes attributes,
        CancellationToken cancellationToken)
    {
        var validator = validatorFactory.CreateValidator<NewUserAttributes>(schema);
        var validationErrors = new List<InputObjectValidationError>();

        if (!validator.Validate(attributes, ["input", "attributes"], validationErrors))
        {
            return new(validationErrors);
        }

        var ipAddress = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress;

        try
        {
            var id = await userManager.CreateUser(
                attributes, claimsPrincipal.GetUserId(), ipAddress, cancellationToken);
            return new CreateUserPayload(id);
        }
        catch (NotUniqueException ex) when (ex.PropertyName == nameof(NewUserAttributes.Email))
        {
            return new(new InputObjectValidationError(
                localizer["Error_EmailNotUnique"],
                ["input", "attributes", "email"],
                ValidationErrorCode.NotUnique));
        }
    }

    /// <summary>
    /// Creates a test user.
    /// </summary>
    /// <remarks>
    /// This mutation is available in development environments only.
    /// </remarks>
    /// <param name="userManager">
    /// The user manager.
    /// </param>
    /// <param name="cancellationToken">
    /// The cancellation token.
    /// </param>
    [Authorize(AuthorizationPolicyNames.AdminOnly)]
    public async Task<FieldResult<CreateTestUserPayload>> CreateTestUser(
        IUserManager userManager, CancellationToken cancellationToken)
    {
        var (id, password) = await userManager.CreateTestUser(cancellationToken);
        return new CreateTestUserPayload(id, password);
    }

    /// <summary>
    /// Deactivates a user.
    /// </summary>
    /// <param name="claimsPrincipal">
    /// The claims principal.
    /// </param>
    /// <param name="httpContextAccessor">
    /// The HTTP context accessor.
    /// </param>
    /// <param name="userManager">
    /// The user manager.
    /// </param>
    /// <param name="id">
    /// The user ID.
    /// </param>
    /// <param name="cancellationToken">
    /// The cancellation token.
    /// </param>
    [Authorize(AuthorizationPolicyNames.AdminOnly)]
    [Error<NotFoundException>]
    public async Task<DeactivateUserPayload> DeactivateUser(
        ClaimsPrincipal claimsPrincipal,
        IHttpContextAccessor httpContextAccessor,
        IUserManager userManager,
        long id,
        CancellationToken cancellationToken)
    {
        var deactivated = await userManager.DeactivateUser(
            id,
            claimsPrincipal.GetUserId(),
            httpContextAccessor.HttpContext?.Connection.RemoteIpAddress,
            cancellationToken);

        return new(id, deactivated);
    }

    /// <summary>
    /// Hard-deletes a test user.
    /// </summary>
    /// <remarks>
    /// This mutation is available in development environments only.
    /// </remarks>
    /// <param name="userManager">
    /// The user manager.
    /// </param>
    /// <param name="id">
    /// The user ID.
    /// </param>
    /// <param name="cancellationToken">
    /// The cancellation token.
    /// </param>
    [Authorize(AuthorizationPolicyNames.AdminOnly)]
    [UseMutationConvention(PayloadTypeName = nameof(HardDeletePayload))]
    public async Task<HardDeletePayload> HardDeleteTestUser(
        IUserManager userManager, long id, CancellationToken cancellationToken) =>
        new(await userManager.HardDeleteTestUser(id, cancellationToken));

    /// <summary>
    /// Reactivates a user.
    /// </summary>
    /// <param name="claimsPrincipal">
    /// The claims principal.
    /// </param>
    /// <param name="httpContextAccessor">
    /// The HTTP context accessor.
    /// </param>
    /// <param name="userManager">
    /// The user manager.
    /// </param>
    /// <param name="id">
    /// The user ID.
    /// </param>
    /// <param name="cancellationToken">
    /// The cancellation token.
    /// </param>
    [Authorize(AuthorizationPolicyNames.AdminOnly)]
    [Error<NotFoundException>]
    public async Task<ReactivateUserPayload> ReactivateUser(
        ClaimsPrincipal claimsPrincipal,
        IHttpContextAccessor httpContextAccessor,
        IUserManager userManager,
        long id,
        CancellationToken cancellationToken)
    {
        var reactivated = await userManager.ReactivateUser(
            id,
            claimsPrincipal.GetUserId(),
            httpContextAccessor.HttpContext?.Connection.RemoteIpAddress,
            cancellationToken);

        return new(id, reactivated);
    }
}
