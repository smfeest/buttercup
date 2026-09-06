using Buttercup.EntityModel;
using HotChocolate.Data.Filters;

namespace Buttercup.Web.Api;

public sealed class CommentRevisionFilterType : FilterInputType<CommentRevision>
{
    protected override void Configure(IFilterInputTypeDescriptor<CommentRevision> descriptor) =>
        descriptor
            .Ignore(r => r.Comment)
            .Ignore(r => r.CommentId);
}
