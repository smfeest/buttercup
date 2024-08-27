using Buttercup.EntityModel;
using HotChocolate.Data.Filters;

namespace Buttercup.Web.Api;

public sealed class CommentFilterType : FilterInputType<Comment>
{
    protected override void Configure(IFilterInputTypeDescriptor<Comment> descriptor) =>
        descriptor
            .Ignore(c => c.RecipeId)
            .Ignore(c => c.AuthorId)
            .Ignore(c => c.DeletedByUserId)
            .Ignore(c => c.Revision)
            .Ignore(c => c.Revisions);
}
