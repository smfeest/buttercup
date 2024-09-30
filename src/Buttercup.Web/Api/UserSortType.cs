using Buttercup.EntityModel;
using HotChocolate.Data.Sorting;

namespace Buttercup.Web.Api;

public sealed class UserSortType : SortInputType<User>
{
    protected override void Configure(ISortInputTypeDescriptor<User> descriptor)
    {
        descriptor.Field(u => u.Email).Directive(AdminOnlyDirectiveType.DirectiveName);
        descriptor.Field(u => u.PasswordCreated).Directive(AdminOnlyDirectiveType.DirectiveName);
        descriptor.Field(u => u.IsAdmin).Directive(AdminOnlyDirectiveType.DirectiveName);
        descriptor
            .Ignore(u => u.HashedPassword)
            .Ignore(u => u.SecurityStamp)
            .Ignore(u => u.Revision);
    }
}
