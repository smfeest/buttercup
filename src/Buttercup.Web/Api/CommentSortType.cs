using Buttercup.EntityModel;
using HotChocolate.Data.Sorting;

namespace Buttercup.Web.Api;

public sealed class CommentSortType : SortInputType<Comment>
{
    protected override void Configure(ISortInputTypeDescriptor<Comment> descriptor) =>
        descriptor
            .Ignore(c => c.RecipeId)
            .Ignore(c => c.AuthorId)
            .Ignore(c => c.Body)
            .Ignore(c => c.DeletedByUserId)
            .Ignore(c => c.Revision)
            .Ignore(c => c.Revisions);
}
