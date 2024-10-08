using System.ComponentModel.DataAnnotations;
using Moq;
using Xunit;

namespace Buttercup.Application.Validation;

public sealed class ValidatorTests
{
    private readonly Validator<SampleModel> validator;

    public ValidatorTests() => this.validator = new(new FakeValidationErrorLocalizer());

    [Fact]
    public void GetValidationErrors_ReturnsTrueWithoutAddingErrorsWhenValid()
    {
        var errors = new List<ValidationError>();
        Assert.True(this.validator.Validate(SampleModel.ValidExample, errors));
        Assert.Empty(errors);
    }

    [Fact]
    public void GetValidationErrors_ReturnsFalseAndAddsErrorWhenClassAttributeNotSatisfied()
    {
        var model = SampleModel.ValidExample with { OptionalNumberWithRange = 5 };
        var errors = new List<ValidationError>();
        Assert.False(this.validator.Validate(model, errors));

        var error = Assert.Single(errors);
        Assert.Equal("Message: SampleModel / OptionalNumberNotOddAttribute", error.Message);
        Assert.Same(model, error.Instance);
        Assert.Null(error.Member);
        Assert.Same(model, error.Value);
        Assert.IsType<OptionalNumberNotOddAttribute>(error.ValidationAttribute);
    }

    [Fact]
    public void GetValidationErrors_ReturnsFalseAndAddsErrorWhenRequiredAttributeNotSatisfied()
    {
        var model = SampleModel.ValidExample with { RequiredString = string.Empty };
        var errors = new List<ValidationError>();
        Assert.False(this.validator.Validate(model, errors));

        var error = Assert.Single(errors);
        Assert.Equal("Message: RequiredString / RequiredAttribute", error.Message);
        Assert.Same(model, error.Instance);
        Assert.Equal(nameof(SampleModel.RequiredString), error.Member);
        Assert.Same(string.Empty, error.Value);
        Assert.IsType<RequiredAttribute>(error.ValidationAttribute);
    }

    [Fact]
    public void GetValidationErrors_SkipsOtherAttributesWhenRequiredAttributeNotSatisfied()
    {
        var model = SampleModel.ValidExample with
        {
            RequiredStringWithAllowedValues = string.Empty
        };
        var errors = new List<ValidationError>();
        this.validator.Validate(model, errors);

        var error = Assert.Single(errors);
        Assert.IsType<RequiredAttribute>(error.ValidationAttribute);
    }

    [Fact]
    public void GetValidationErrors_RunsOtherAttributesWhenRequiredAttributeSatisfied()
    {
        var model = SampleModel.ValidExample with { RequiredStringWithAllowedValues = "Blue" };
        var errors = new List<ValidationError>();
        this.validator.Validate(model, errors);

        var error = Assert.Single(errors);
        Assert.IsType<AllowedValuesAttribute>(error.ValidationAttribute);
    }

    [Fact]
    public void GetValidationErrors_ReturnsFalseAndAddsErrorWhenOtherPropertyAttributeNotSatisfied()
    {
        var model = SampleModel.ValidExample with { OptionalNumberWithRange = 8 };

        var errors = new List<ValidationError>();
        Assert.False(this.validator.Validate(model, errors));

        var error = Assert.Single(errors);
        Assert.Equal("Message: OptionalNumberWithRange / RangeAttribute", error.Message);
        Assert.Same(model, error.Instance);
        Assert.Equal(nameof(SampleModel.OptionalNumberWithRange), error.Member);
        Assert.Equal(8, error.Value);
        Assert.IsType<RangeAttribute>(error.ValidationAttribute);
    }

    [Fact]
    public void GetValidationErrors_AddsAllErrorsWhenInvalid()
    {
        var model = new SampleModel
        {
            OptionalNumberWithRange = 7, // odd + out of range
            RequiredString = null, // not specified
            RequiredStringWithAllowedValues = "Yellow" // not allowed + too long
        };
        var errors = new List<ValidationError>();
        this.validator.Validate(model, errors);

        Assert.Equal(5, errors.Count);
    }

    [OptionalNumberNotOdd]
    public sealed record SampleModel
    {
        [Range(3, 5)]
        public int? OptionalNumberWithRange { get; set; }

        public string? OptionalStringWithoutValidation { get; set; }

        [Required]
        public string? RequiredString { get; set; }

        [AllowedValues("Red", "Green")]
        [MaxLength(5)]
        [Required]
        public string? RequiredStringWithAllowedValues { get; set; }

        public static SampleModel ValidExample { get; } = new()
        {
            OptionalNumberWithRange = 4,
            RequiredString = "Square",
            RequiredStringWithAllowedValues = "Red",
        };
    }

    private sealed class OptionalNumberNotOddAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(
            object? value, ValidationContext validationContext) =>
            value is SampleModel sampleModel && sampleModel.OptionalNumberWithRange % 2 == 1
                ? new ValidationResult(null) : ValidationResult.Success;
    }

    private sealed class FakeValidationErrorLocalizer : IValidationErrorLocalizer<SampleModel>
    {
        public string FormatMessage(
            ValidationContext validationContext, ValidationAttribute attribute) =>
            $"Message: {validationContext.DisplayName} / {attribute.GetType().Name}";
    }
}
