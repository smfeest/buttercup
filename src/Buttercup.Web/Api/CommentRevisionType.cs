using Buttercup.EntityModel;

namespace Buttercup.Web.Api;

public sealed class CommentRevisionType : ObjectType<CommentRevision>
{
    protected override void Configure(IObjectTypeDescriptor<CommentRevision> descriptor) =>
        descriptor.Ignore(r => r.CommentId);
}
