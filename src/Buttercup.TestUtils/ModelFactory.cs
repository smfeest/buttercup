using Buttercup.Models;

namespace Buttercup.TestUtils;

public static class ModelFactory
{
    private static int counter;

    public static Recipe CreateRecipe(
        bool includeOptionalAttributes = false,
        long? id = null,
        string? title = null,
        int? revision = null)
    {
        var i = Interlocked.Increment(ref counter);

        return new(
            id ?? i,
            title ?? $"recipe-{i}-title",
            includeOptionalAttributes ? i + 1 : null,
            includeOptionalAttributes ? i + 2 : null,
            includeOptionalAttributes ? i + 3 : null,
            $"recipe-{i}-ingredients",
            $"recipe-{i}-method",
            includeOptionalAttributes ? $"recipe-{i}-suggestions" : null,
            includeOptionalAttributes ? $"recipe-{i}-remarks" : null,
            includeOptionalAttributes ? $"recipe-{i}-source" : null,
            new DateTime(2001, 2, 3, 4, 5, 6).AddSeconds(i),
            includeOptionalAttributes ? i + 4 : null,
            new DateTime(2002, 3, 4, 5, 6, 7).AddSeconds(i),
            includeOptionalAttributes ? i + 5 : null,
            revision ?? (i + 4));
    }

    public static User CreateUser(
        bool includeOptionalAttributes = false,
        long? id = null,
        string? email = null,
        string? securityStamp = null,
        string? timeZone = null,
        int? revision = null)
    {
        var i = Interlocked.Increment(ref counter);

        return new(
            id ?? i,
            $"user-{i}-name",
            email ?? $"user-{i}@example.com",
            includeOptionalAttributes ? $"user-{i}-password" : null,
            includeOptionalAttributes ? new DateTime(2000, 1, 2, 3, 4, 5).AddSeconds(i) : null,
            securityStamp ?? "secstamp",
            timeZone ?? $"user-{i}-time-zone",
            new DateTime(2001, 2, 3, 4, 5, 6).AddSeconds(i),
            new DateTime(2002, 3, 4, 5, 6, 7).AddSeconds(i),
            revision ?? (i + 1));
    }
}
