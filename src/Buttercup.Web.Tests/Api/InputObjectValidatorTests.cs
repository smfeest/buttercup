using System.ComponentModel.DataAnnotations;
using Buttercup.Application.Validation;
using HotChocolate;
using Moq;
using Xunit;

namespace Buttercup.Web.Api;

public sealed class InputObjectValidatorTests
{
    private readonly Planet planet = new(1.2, 2.3);
    private readonly Mock<IValidator<Planet>> validatorMock = new();

    [Fact]
    public void Validate_ReturnsTrueWhenValid()
    {
        this.SetupValid();
        var inputObjectValidator = this.CreateInputObjectValidator();

        Assert.True(inputObjectValidator.Validate(this.planet, [], []));
    }

    [Fact]
    public void Validate_DoesNotQuerySchemaWhenValid()
    {
        this.SetupValid();
        var schemaMock = new Mock<ISchema>();
        var inputObjectValidator = this.CreateInputObjectValidator(schemaMock.Object);

        inputObjectValidator.Validate(this.planet, [], []);

        schemaMock.Verify(x => x.Types, Times.Never);
    }

    [Fact]
    public void Validate_ReturnsFalseWhenInvalid()
    {
        this.SetupInvalid();
        var inputObjectValidator = this.CreateInputObjectValidator();

        Assert.False(inputObjectValidator.Validate(this.planet, [], []));
    }

    [Fact]
    public void Validate_PassesThroughMessageFromValidationErrors()
    {
        this.SetupInvalid("Planet is too far from sun");
        var inputObjectValidator = this.CreateInputObjectValidator();
        var errors = new List<InputObjectValidationError>();

        inputObjectValidator.Validate(this.planet, [], errors);

        var error = Assert.Single(errors);
        Assert.Equal("Planet is too far from sun", error.Message);
    }

    [Fact]
    public void Validate_SetsFieldPathToBasePathWhenMemberIsNull()
    {
        this.SetupInvalid(member: null);
        var inputObjectValidator = this.CreateInputObjectValidator();
        var basePath = new string[] { "foo", "bar" };
        var errors = new List<InputObjectValidationError>();

        inputObjectValidator.Validate(this.planet, basePath, errors);

        var error = Assert.Single(errors);
        Assert.Equal(basePath, error.Path);
    }

    [Theory]
    [InlineData(nameof(Planet.AverageDiameter), "diameter")]
    [InlineData(nameof(Planet.Mass), "mass")]
    public void Validate_AppendsCorrespondingFieldNameToBasePathWhenMemberIsNotNull(
        string member, string field)
    {
        this.SetupInvalid(member: member);
        var inputObjectValidator = this.CreateInputObjectValidator();
        var errors = new List<InputObjectValidationError>();

        inputObjectValidator.Validate(this.planet, ["foo", "bar"], errors);

        var error = Assert.Single(errors);
        Assert.Equal(new string[] { "foo", "bar", field }, error.Path);
    }

    [Fact]
    public void Validate_SetsCodeToInvalidFormatWhenErrorIsFromEmailAddressAttribute()
    {
        this.SetupInvalid(attribute: new EmailAddressAttribute());
        var inputObjectValidator = this.CreateInputObjectValidator();
        var errors = new List<InputObjectValidationError>();

        inputObjectValidator.Validate(this.planet, [], errors);

        var error = Assert.Single(errors);
        Assert.Equal(ValidationErrorCode.InvalidFormat, error.Code);
    }

    [Fact]
    public void Validate_SetsCodeToOutOfRangeWhenErrorIsFromRangeAttribute()
    {
        this.SetupInvalid(attribute: new RangeAttribute(1, 2));
        var inputObjectValidator = this.CreateInputObjectValidator();
        var errors = new List<InputObjectValidationError>();

        inputObjectValidator.Validate(this.planet, [], errors);

        var error = Assert.Single(errors);
        Assert.Equal(ValidationErrorCode.OutOfRange, error.Code);
    }

    [Fact]
    public void Validate_SetsCodeToRequiredWhenErrorIsFromRequiredAttribute()
    {
        this.SetupInvalid(attribute: new RequiredAttribute());
        var inputObjectValidator = this.CreateInputObjectValidator();
        var errors = new List<InputObjectValidationError>();

        inputObjectValidator.Validate(this.planet, [], errors);

        var error = Assert.Single(errors);
        Assert.Equal(ValidationErrorCode.Required, error.Code);
    }

    [Fact]
    public void Validate_SetsCodeToInvalidStringLengthWhenErrorIsFromStringLengthAttribute()
    {
        this.SetupInvalid(attribute: new StringLengthAttribute(5));
        var inputObjectValidator = this.CreateInputObjectValidator();
        var errors = new List<InputObjectValidationError>();

        inputObjectValidator.Validate(this.planet, [], errors);

        var error = Assert.Single(errors);
        Assert.Equal(ValidationErrorCode.InvalidStringLength, error.Code);
    }

    [Fact]
    public void Validation_ThrowsNotSupportedExceptionWhenNoCodeIsMappedToAttribute()
    {
        this.SetupInvalid(attribute: new Base64StringAttribute());
        var inputObjectValidator = this.CreateInputObjectValidator();

        var exception = Assert.Throws<NotSupportedException>(
            () => inputObjectValidator.Validate(this.planet, [], []));

        Assert.Equal(
            $"No ValidationErrorCode mapping exists for validation attribute type '{nameof(Base64StringAttribute)}'",
            exception.Message);
    }

    private InputObjectValidator<Planet> CreateInputObjectValidator() =>
        this.CreateInputObjectValidator(CreateSchema());

    private InputObjectValidator<Planet> CreateInputObjectValidator(ISchema schema) =>
        new(schema, this.validatorMock.Object);

    private static ISchema CreateSchema() =>
        new SchemaBuilder()
            .ModifyOptions(options => options.StrictValidation = false)
            .AddInputObjectType<Galaxy>()
            .AddInputObjectType<Planet>()
            .AddInputObjectType<Star>()
            .Create();

    private void SetupValid() =>
        this.validatorMock
            .Setup(x => x.Validate(this.planet, It.IsAny<List<ValidationError>>()))
            .Returns(true);

    private void SetupInvalid(
        string? message = null, string? member = null, ValidationAttribute? attribute = null) =>
        this.validatorMock
            .Setup(x => x.Validate(this.planet, It.IsAny<List<ValidationError>>()))
            .Callback((Planet _, ICollection<ValidationError> errors) =>
            {
                errors.Add(new(
                    message ?? "Invalid value",
                    new(),
                    member,
                    null,
                    attribute ?? new RangeAttribute(2, 3)));
            })
            .Returns(false);

    public sealed record Galaxy(string Constellation);

    public sealed record Planet(
        [property: GraphQLName("diameter")] double AverageDiameter, double Mass);

    public sealed record Star(string Constellation);
}
