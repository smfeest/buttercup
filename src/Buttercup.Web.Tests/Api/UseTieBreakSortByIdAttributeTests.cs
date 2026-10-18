using Buttercup.EntityModel;
using HotChocolate;
using HotChocolate.Data;
using HotChocolate.Execution;
using HotChocolate.Text.Json;
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
        var data = result.ExpectOperationResult().Data;
        Assert.NotNull(data);
        var resultDocument = Assert.IsType<ResultDocument>(data.Value.Value);
        var actualOrderedIds = resultDocument.Data.GetProperty("foos").EnumerateArray().Select(
            item => item.GetProperty("id").GetInt64());
        Assert.Equal(expectedOrderedIds, actualOrderedIds);
    }

    private static async Task<IExecutionResult> Execute(string query)
    {
        var executor = await new ServiceCollection()
            .AddGraphQLServer()
            .AddQueryType<Query>()
            .AddSorting()
            .BuildRequestExecutorAsync();

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
