using System.ComponentModel.DataAnnotations;
using Buttercup.Application.Validation;

namespace Buttercup.Web.Api;

public sealed class InputObjectValidator<T>(ISchema schema, IValidator<T> validator)
    : IInputObjectValidator<T> where T : notnull
{
    private readonly Lazy<Dictionary<string, string>> propertyMappings =
        new(() => BuildPropertyMappings(schema), LazyThreadSafetyMode.None);
    private readonly IValidator<T> validator = validator;

    public bool Validate(
        T instance, string[] basePath, ICollection<InputObjectValidationError> validationErrors)
    {
        var internalErrors = new List<ValidationError>();

        if (!this.validator.Validate(instance, internalErrors))
        {
            foreach (var internalError in internalErrors)
            {
                var path = internalError.Member is not null ?
                    [.. basePath, this.propertyMappings.Value[internalError.Member]] :
                    basePath;
                var code = ResolveCode(internalError.ValidationAttribute);

                validationErrors.Add(new(internalError.Message, path, code));
            }

            return false;
        }

        return true;
    }

    private static Dictionary<string, string> BuildPropertyMappings(ISchema schema)
    {
        var inputType = schema.Types.OfType<IInputObjectType>()
            .First(type => type.RuntimeType == typeof(T));

        var mappings = new Dictionary<string, string>();

        foreach (var field in inputType.Fields.OfType<InputField>())
        {
            if (field.Property is not null)
            {
                mappings.Add(field.Property.Name, field.Name);
            }
        }

        return mappings;
    }

    private static ValidationErrorCode ResolveCode(ValidationAttribute attribute) =>
        attribute switch
        {
            EmailAddressAttribute => ValidationErrorCode.InvalidFormat,
            RangeAttribute => ValidationErrorCode.OutOfRange,
            RequiredAttribute required => ValidationErrorCode.Required,
            StringLengthAttribute length => ValidationErrorCode.InvalidStringLength,
            _ => throw new NotSupportedException(
                $"No {nameof(ValidationErrorCode)} mapping exists for validation attribute type '{attribute.GetType().Name}'")
        };
}
