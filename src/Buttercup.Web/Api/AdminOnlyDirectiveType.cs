using Buttercup.Security;

namespace Buttercup.Web.Api;

/// <summary>
/// Represents the directive used to mark a sort field as only available to users with the <see
/// cref="RoleNames.Admin"/> role.
/// </summary>
public sealed class AdminOnlyDirectiveType : DirectiveType
{
    public const string DirectiveName = "adminOnly";

    protected override void Configure(IDirectiveTypeDescriptor descriptor) =>
        descriptor
            .Name(DirectiveName)
            .Location(DirectiveLocation.InputFieldDefinition);
}
