using Buttercup.EntityModel;
using Microsoft.Extensions.Localization;

namespace Buttercup.Web.Globalization;

public sealed class RoleLocalizer(IStringLocalizer<RoleLocalizer> localizer) : IRoleLocalizer
{
    private readonly IStringLocalizer<RoleLocalizer> localizer = localizer;

    public string this[Role role] => this.localizer[role.ToString()];
}
