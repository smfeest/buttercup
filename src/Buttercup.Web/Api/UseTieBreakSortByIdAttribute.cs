
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Buttercup.EntityModel;
using HotChocolate.Types.Descriptors;

namespace Buttercup.Web.Api;

public class UseTieBreakSortByIdAttribute<T> : ObjectFieldDescriptorAttribute where T : IEntityId
{
    public UseTieBreakSortByIdAttribute([CallerLineNumber] int order = 0) => this.Order = order;

    protected override void OnConfigure(
        IDescriptorContext context, IObjectFieldDescriptor descriptor, MemberInfo member) =>
        descriptor.Use(next => async middlewareContext =>
        {
            await next(middlewareContext);

            if (middlewareContext.Result is IOrderedQueryable<T> queryable)
            {
                var visitor = new IsOrderedVisitor();
                visitor.Visit(queryable.Expression);

                middlewareContext.Result = visitor.HasOrderBy ?
                    queryable.ThenBy(e => e.Id) :
                    queryable.OrderBy(e => e.Id);
            }
        });

    private sealed class IsOrderedVisitor : ExpressionVisitor
    {
        public bool HasOrderBy { get; private set; }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (!this.HasOrderBy && node.Method.Name switch
            {
                nameof(Enumerable.OrderBy) => true,
                nameof(Enumerable.OrderByDescending) => true,
                _ => false,
            })
            {
                this.HasOrderBy = true;
            }

            return base.VisitMethodCall(node);
        }
    }
}
