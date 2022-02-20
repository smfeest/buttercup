using Moq;
using Xunit;

namespace Buttercup.DataAccess;

[Collection("Database collection")]
public class UserDataProviderTests
{
    private readonly DateTime fakeTime = new(2020, 1, 2, 3, 4, 5);
    private readonly UserDataProvider userDataProvider;

    public UserDataProviderTests()
    {
        var clock = Mock.Of<IClock>(x => x.UtcNow == this.fakeTime);

        this.userDataProvider = new(clock);
    }

    #region FindUserByEmail

    [Fact]
    public async Task FindUserByEmailReturnsUser()
    {
        using var connection = await TestDatabase.OpenConnectionWithRollback();

        var expected = await new SampleDataHelper(connection).InsertUser();

        var actual = await this.userDataProvider.FindUserByEmail(connection, expected.Email);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task FindUserByEmailReturnsNullIfNoMatchFound()
    {
        using var connection = await TestDatabase.OpenConnectionWithRollback();

        await new SampleDataHelper(connection).InsertUser();

        var actual = await this.userDataProvider.FindUserByEmail(
            connection, "non-existent@example.com");

        Assert.Null(actual);
    }

    #endregion

    #region GetUser

    [Fact]
    public async Task GetUserReturnsUser()
    {
        using var connection = await TestDatabase.OpenConnectionWithRollback();

        var expected = await new SampleDataHelper(connection).InsertUser();

        var actual = await this.userDataProvider.GetUser(connection, expected.Id);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task GetUserThrowsIfRecordNotFound()
    {
        using var connection = await TestDatabase.OpenConnectionWithRollback();

        var otherUser = await new SampleDataHelper(connection).InsertUser();

        var id = otherUser.Id + 1;

        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => this.userDataProvider.GetUser(connection, id));

        Assert.Equal($"User {id} not found", exception.Message);
    }

    #endregion

    #region GetUsers

    [Fact]
    public async Task GetUsersReturnsUsersWithMatchingIds()
    {
        using var connection = await TestDatabase.OpenConnectionWithRollback();

        var sampleDataHelper = new SampleDataHelper(connection);

        var allUsers = new[]
        {
            await sampleDataHelper.InsertUser(),
            await sampleDataHelper.InsertUser(),
            await sampleDataHelper.InsertUser(),
        };

        var expected = new[] { allUsers[0], allUsers[2] };

        var actual = await this.userDataProvider.GetUsers(
            connection, new[] { allUsers[0].Id, allUsers[2].Id });

        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task GetUsersReturnsEmptyListIdListIsEmpty()
    {
        using var connection = await TestDatabase.OpenConnectionWithRollback();

        Assert.Empty(await this.userDataProvider.GetUsers(connection, Array.Empty<long>()));
    }

    #endregion

    #region UpdatePassword

    [Fact]
    public async Task UpdatePasswordUpdatesHashedPassword()
    {
        using var connection = await TestDatabase.OpenConnectionWithRollback();

        var original = await new SampleDataHelper(connection).InsertUser();

        await this.userDataProvider.UpdatePassword(
            connection, original.Id, "new-hashed-password", "newstamp");

        var expected = original with
        {
            HashedPassword = "new-hashed-password",
            PasswordCreated = this.fakeTime,
            SecurityStamp = "newstamp",
            Modified = this.fakeTime,
            Revision = original.Revision + 1,
        };

        var actual = await this.userDataProvider.GetUser(connection, original.Id);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task UpdatePasswordThrowsIfRecordNotFound()
    {
        using var connection = await TestDatabase.OpenConnectionWithRollback();

        var otherUser = await new SampleDataHelper(connection).InsertUser();

        var id = otherUser.Id + 1;

        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => this.userDataProvider.UpdatePassword(
                connection, id, "new-hashed-password", "newstamp"));

        Assert.Equal($"User {id} not found", exception.Message);
    }

    #endregion

    #region UpdatePreferences

    [Fact]
    public async Task UpdatePreferencesUpdatesPreferences()
    {
        using var connection = await TestDatabase.OpenConnectionWithRollback();

        var original = await new SampleDataHelper(connection).InsertUser();

        await this.userDataProvider.UpdatePreferences(connection, original.Id, "new-time-zone");

        var expected = original with
        {
            TimeZone = "new-time-zone",
            Modified = this.fakeTime,
            Revision = original.Revision + 1,
        };

        var actual = await this.userDataProvider.GetUser(connection, original.Id);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task UpdatePreferencesThrowsIfRecordNotFound()
    {
        using var connection = await TestDatabase.OpenConnectionWithRollback();

        var otherUser = await new SampleDataHelper(connection).InsertUser();

        var id = otherUser.Id + 1;

        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => this.userDataProvider.UpdatePreferences(connection, id, "new-time-zone"));

        Assert.Equal($"User {id} not found", exception.Message);
    }

    #endregion

    #region ReadUser

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ReadUserReadsAllAttributes(bool includeOptionalAttributes)
    {
        using var connection = await TestDatabase.OpenConnectionWithRollback();

        var expected = await new SampleDataHelper(connection).InsertUser(includeOptionalAttributes);

        var actual = await this.userDataProvider.GetUser(connection, expected.Id);

        Assert.Equal(expected, actual);
    }

    #endregion
}
