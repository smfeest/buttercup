using Buttercup.Models;

namespace Buttercup.DataAccess;

public static class SampleUsers
{
    private static int sampleUserCount;

    public static User CreateSampleUser(
        bool includeOptionalAttributes = false,
        long? id = null,
        string? email = null,
        int? revision = null)
    {
        var i = ++sampleUserCount;

        var user = new User
        {
            Id = id ?? i,
            Name = $"user-{i}-name",
            Email = email ?? $"user-{i}@example.com",
            SecurityStamp = "secstamp",
            TimeZone = $"user-{i}-time-zone",
            Created = new DateTime(2001, 2, 3, 4, 5, 6).AddSeconds(i),
            Modified = new DateTime(2002, 3, 4, 5, 6, 7).AddSeconds(i),
            Revision = revision ?? (i + 1),
        };

        if (includeOptionalAttributes)
        {
            user.HashedPassword = $"user-{i}-password";
            user.PasswordCreated = new DateTime(2000, 1, 2, 3, 4, 5).AddSeconds(i);
        }

        return user;
    }
}
