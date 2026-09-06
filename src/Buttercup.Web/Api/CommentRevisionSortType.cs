using Buttercup.EntityModel;
using HotChocolate.Data.Sorting;

namespace Buttercup.Web.Api;

public sealed class CommentRevisionSortType : SortInputType<CommentRevision>
{
    protected override void Configure(ISortInputTypeDescriptor<CommentRevision> descriptor) =>
        descriptor
            .Ignore(r => r.Comment)
            .Ignore(r => r.CommentId);
}
