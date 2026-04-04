using Buttercup.EntityModel;
using Buttercup.TestUtils;
using Xunit;

namespace Buttercup.Web.Globalization;

public sealed class RoleLocalizerTests
{
    [Fact]
    public void ReturnsLocalizedRoleName()
    {
        var stringLocalizer = new DictionaryLocalizer<RoleLocalizer>()
            .Add("Admin", "Admin-Localized")
            .Add("Contributor", "Contributor-Localized");

        var localizer = new RoleLocalizer(stringLocalizer);

        Assert.Equal("Admin-Localized", localizer[Role.Admin]);
        Assert.Equal("Contributor-Localized", localizer[Role.Contributor]);
    }
}
