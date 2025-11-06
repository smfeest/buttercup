using Buttercup.Application;
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
    [Authorize(AuthorizationPolicyNames.AdminOnly)]
    public async Task<FieldResult<CreateUserPayload, InputObjectValidationError>> CreateUser(
        IInputObjectValidatorFactory validatorFactory,
        IUserManager userManager,
        ISchema schema,
        IStringLocalizer<UserMutations> localizer,
        NewUserAttributes attributes)
    {
        var validator = validatorFactory.CreateValidator<NewUserAttributes>(schema);
        var validationErrors = new List<InputObjectValidationError>();

        if (!validator.Validate(attributes, ["input", "attributes"], validationErrors))
        {
            return new(validationErrors);
        }

        try
        {
            var id = await userManager.CreateUser(attributes);
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
}
