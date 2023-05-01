using Microsoft.Extensions.Configuration;
using Xunit;

namespace Buttercup.Web;

public class ConfigurationExtensionsTests
{
    [Fact]
    public void GetRequiredConnectionStringReturnsSpecifiedConnectionString() =>
        Assert.Equal(
            "ConnectionString2", BuildConfiguration().GetRequiredConnectionString("Name2"));

    [Fact]
    public void GetRequiredConnectionStringThrowsIfSpecifiedConnectionStringIsMissing() =>
        Assert.Throws<InvalidOperationException>(
            () => BuildConfiguration().GetRequiredConnectionString("Name3"));

    private static IConfiguration BuildConfiguration() => new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>()
        {
            ["ConnectionStrings:Name1"] = "ConnectionString1",
            ["ConnectionStrings:Name2"] = "ConnectionString2",
        })
        .Build();
}
