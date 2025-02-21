using Buttercup.EntityModel;
using HotChocolate.Data;
using HotChocolate.Execution;
using HotChocolate.Execution.Processing;
using Xunit;

namespace Buttercup.Web.Api;

public sealed class UseTieBreakSortByIdAttributeTests
{
    [Theory]
    [InlineData("{ foos { id } }", new long[] { 1, 2, 3, 4, 5 })]
    [InlineData("{ foos(order: { bar: ASC }) { id } }", new long[] { 3, 1, 5, 2, 4 })]
    [InlineData("{ foos(order: { bar: DESC }) { id } }", new long[] { 2, 4, 1, 5, 3 })]
    public async Task TieBreaksSortById(string query, long[] expectedOrderedIds)
    {
        var result = await Execute(query);
        var actualOrderedIds = ((ListResult)result.ExpectOperationResult().Data!["foos"]!).Select(
            listItem => (long)((IReadOnlyDictionary<string, object?>)listItem!)["id"]!);
        Assert.Equal(expectedOrderedIds, actualOrderedIds);
    }

    private static async Task<IExecutionResult> Execute(string query)
    {
        var serviceProvider = new ServiceCollection()
            .AddGraphQLServer()
            .AddQueryType<Query>()
            .AddSorting(convention => convention.AddDefaults())
            .Services
            .BuildServiceProvider();

        var executor = await serviceProvider.GetRequestExecutorAsync();

        return await executor.ExecuteAsync(query);
    }

    public sealed record Foo(long Id, string Bar) : IEntityId;

    public sealed class Query
    {
        [UseTieBreakSortById<Foo>]
        [UseSorting]
        public IQueryable<Foo> Foos() => new Foo[]
        {
            new(3, "Alpha"),
            new(4, "Charlie"),
            new(1, "Bravo"),
            new(5, "Bravo"),
            new(2, "Charlie"),
        }.AsQueryable();
    }
}
