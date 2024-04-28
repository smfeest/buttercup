using Buttercup.EntityModel;

namespace Buttercup.Web.Api;

public sealed class CommentType : ObjectType<Comment>
{
    protected override void Configure(IObjectTypeDescriptor<Comment> descriptor) =>
        descriptor
            .Ignore(c => c.RecipeId)
            .Ignore(c => c.AuthorId)
            .Ignore(c => c.DeletedByUserId);
}
