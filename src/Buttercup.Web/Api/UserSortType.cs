using Buttercup.EntityModel;
using HotChocolate.Data.Sorting;

namespace Buttercup.Web.Api;

public sealed class UserSortType : SortInputType<User>
{
    protected override void Configure(ISortInputTypeDescriptor<User> descriptor)
    {
        descriptor.BindFieldsExplicitly();
        descriptor.Field(u => u.Id);
        descriptor.Field(u => u.Name);
        descriptor.Field(u => u.Email);
        descriptor.Field(u => u.Created);
        descriptor.Field(u => u.Modified);
    }
}
