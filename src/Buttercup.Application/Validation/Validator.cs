using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Buttercup.Application.Validation;

internal sealed class Validator<T>(IValidationErrorLocalizer<T> errorLocalizer)
    : IValidator<T> where T : notnull
{
    private readonly IValidationErrorLocalizer<T> errorLocalizer = errorLocalizer;

    private readonly List<ValidationAttribute> objectAttributes = GetObjectAttributes();
    private readonly List<ValidatedProperty> validatedProperties = GetValidatedProperties();

    public bool Validate(T instance, ICollection<ValidationError> validationErrors)
    {
        var validationContext = new ValidationContext(instance);

        var valid = true;

        foreach (var attribute in this.objectAttributes)
        {
            if (!this.TryValidate(validationContext, attribute, instance, validationErrors))
            {
                valid = false;
            }
        }

        foreach (var property in this.validatedProperties)
        {
            if (!this.TryValidateProperty(instance, validationContext, property, validationErrors))
            {
                valid = false;
            }
        }

        return valid;
    }

    private static List<ValidationAttribute> GetObjectAttributes() =>
        [.. TypeDescriptor.GetAttributes(typeof(T)).OfType<ValidationAttribute>()];

    private static List<ValidatedProperty> GetValidatedProperties()
    {
        var properties = TypeDescriptor.GetProperties(typeof(T));

        var validatedProperties = new List<ValidatedProperty>();

        foreach (PropertyDescriptor property in properties)
        {
            var attributes = property.Attributes.OfType<ValidationAttribute>();

            var validatable = false;

            RequiredAttribute? requiredAttribute = null;
            var otherAttributes = new List<ValidationAttribute>();

            foreach (var attribute in attributes)
            {
                validatable = true;

                if (attribute is RequiredAttribute required)
                {
                    requiredAttribute = required;
                }
                else
                {
                    otherAttributes.Add(attribute);
                }
            }

            if (validatable)
            {
                validatedProperties.Add(new(property, requiredAttribute, otherAttributes));
            }
        }

        return validatedProperties;
    }

    private bool TryValidate(
        ValidationContext validationContext,
        ValidationAttribute attribute,
        object? value,
        ICollection<ValidationError> validationErrors)
    {
        var validationResult = attribute.GetValidationResult(value, validationContext);

        if (validationResult is null)
        {
            return true;
        }

        validationErrors.Add(new(
            this.errorLocalizer.FormatMessage(validationContext, attribute),
            validationContext.ObjectInstance,
            validationContext.MemberName,
            value,
            attribute));

        return false;
    }

    private bool TryValidateProperty(
        T instance,
        ValidationContext validationContext,
        ValidatedProperty property,
        ICollection<ValidationError> errors)
    {
        var propertyValidationContext = new ValidationContext(
            validationContext.ObjectInstance, validationContext, validationContext.Items)
        {
            MemberName = property.Name
        };

        var value = property.GetValue(instance);

        if (property.RequiredAttribute is not null &&
            !this.TryValidate(propertyValidationContext, property.RequiredAttribute, value, errors))
        {
            return false;
        }

        var valid = true;

        foreach (var attribute in property.OtherAttributes)
        {
            if (!this.TryValidate(propertyValidationContext, attribute, value, errors))
            {
                valid = false;
            }
        }

        return valid;
    }

    private sealed record ValidatedProperty(
        PropertyDescriptor Property,
        RequiredAttribute? RequiredAttribute,
        List<ValidationAttribute> OtherAttributes)
    {
        public string Name => this.Property.Name;

        public object? GetValue(T instance) => this.Property.GetValue(instance);
    }
}
