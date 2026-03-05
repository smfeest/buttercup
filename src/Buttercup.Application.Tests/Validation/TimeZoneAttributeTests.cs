using System.ComponentModel.DataAnnotations;
using Xunit;

namespace Buttercup.Application.Validation;

public sealed class TimeZoneAttributeTests
{
    [Fact]
    public void GetValidationResult_ValueIsNull_ReturnsSuccess() =>
        Assert.Equal(ValidationResult.Success, Validate(null));

    [Fact]
    public void GetValidationResult_ValueIsRecognizedIanaTimeZoneIdentifier_ReturnsSuccess()
    {
        var timeZoneInfo = TimeZoneInfo.GetSystemTimeZones().First(timeZone => timeZone.HasIanaId);
        Assert.Equal(ValidationResult.Success, Validate(timeZoneInfo.Id));
    }

    [Fact]
    public void GetValidationResult_ValueIsNotRecognizedIanaTimeZoneIdentifier_ReturnsError()
    {
        var result = Validate("Not/A_Real_Time_Zone");

        Assert.NotNull(result);
        Assert.Equal(
            "Not/A_Real_Time_Zone is not a recognized IANA time zone identifier",
            result.ErrorMessage);
    }

    [Fact]
    public void GetValidationResult_ValueIsNotString_ReturnsError()
    {
        var result = Validate(123);

        Assert.NotNull(result);
        Assert.Equal("Time zone identifier must be a string", result.ErrorMessage);
    }

    private static ValidationResult? Validate(object? value) =>
        new TimeZoneAttribute().GetValidationResult(value, new(new object()));
}
