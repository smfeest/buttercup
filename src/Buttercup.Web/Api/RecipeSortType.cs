using Buttercup.EntityModel;
using HotChocolate.Data.Sorting;

namespace Buttercup.Web.Api;

public sealed class RecipeSortType : SortInputType<Recipe>
{
    protected override void Configure(ISortInputTypeDescriptor<Recipe> descriptor)
    {
        descriptor.BindFieldsExplicitly();
        descriptor.Field(r => r.Id);
        descriptor.Field(r => r.Title);
        descriptor.Field(r => r.Created);
        descriptor.Field(r => r.Modified);
        descriptor.Field(r => r.Deleted);
    }
}
