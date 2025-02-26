using System.Reflection;
using System.Runtime.CompilerServices;
using Buttercup.EntityModel;
using HotChocolate.Data.Sorting;
using HotChocolate.Types.Descriptors;

namespace Buttercup.Web.Api;

public class UseTieBreakSortByIdAttribute<T> : ObjectFieldDescriptorAttribute where T : IEntityId
{
    public UseTieBreakSortByIdAttribute([CallerLineNumber] int order = 0) => this.Order = order;

    protected override void OnConfigure(
        IDescriptorContext context, IObjectFieldDescriptor descriptor, MemberInfo member) =>
        descriptor.Use(next => async middlewareContext =>
        {
            var sortingContext = middlewareContext.GetSortingContext();

            if (sortingContext is not null)
            {
                sortingContext.Handled(false);
                sortingContext.OnAfterSortingApplied<IQueryable<T>>(
                    static (sortingApplied, queryable) =>
                        sortingApplied && queryable is IOrderedQueryable<T> orderedQueryable
                            ? orderedQueryable.ThenBy(e => e.Id)
                            : queryable.OrderBy(e => e.Id));
            }

            await next(middlewareContext);
        });
}
