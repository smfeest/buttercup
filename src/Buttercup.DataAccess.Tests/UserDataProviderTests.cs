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

        await new SampleDataHelper(connection).InsertUser(
            ModelFactory.CreateUser(id: 4, email: "alpha@example.com"));

        var actual = await this.userDataProvider.FindUserByEmail(connection, "alpha@example.com");

        Assert.Equal(4, actual!.Id);
        Assert.Equal("alpha@example.com", actual.Email);
    }

    [Fact]
    public async Task FindUserByEmailReturnsNullIfNoMatchFound()
    {
        using var connection = await TestDatabase.OpenConnectionWithRollback();

        await new SampleDataHelper(connection).InsertUser(
            ModelFactory.CreateUser(email: "alpha@example.com"));

        var actual = await this.userDataProvider.FindUserByEmail(connection, "beta@example.com");

        Assert.Null(actual);
    }

    #endregion

    #region GetUser

    [Fact]
    public async Task GetUserReturnsUser()
    {
        using var connection = await TestDatabase.OpenConnectionWithRollback();

        var expected = await new SampleDataHelper(connection).InsertUser(
            ModelFactory.CreateUser(id: 76));

        var actual = await this.userDataProvider.GetUser(connection, 76);

        Assert.Equal(76, actual.Id);
        Assert.Equal(expected.Email, actual.Email);
    }

    [Fact]
    public async Task GetUserThrowsIfRecordNotFound()
    {
        using var connection = await TestDatabase.OpenConnectionWithRollback();

        await new SampleDataHelper(connection).InsertUser(ModelFactory.CreateUser(id: 98));

        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => this.userDataProvider.GetUser(connection, 7));

        Assert.Equal("User 7 not found", exception.Message);
    }

    #endregion

    #region UpdatePassword

    [Fact]
    public async Task UpdatePasswordUpdatesHashedPassword()
    {
        using var connection = await TestDatabase.OpenConnectionWithRollback();

        await new SampleDataHelper(connection).InsertUser(
            ModelFactory.CreateUser(id: 41, revision: 5));

        await this.userDataProvider.UpdatePassword(
            connection, 41, "new-hashed-password", "newstamp");

        var actual = await this.userDataProvider.GetUser(connection, 41);

        Assert.Equal("new-hashed-password", actual.HashedPassword);
        Assert.Equal(this.fakeTime, actual.PasswordCreated);
        Assert.Equal("newstamp", actual.SecurityStamp);
        Assert.Equal(this.fakeTime, actual.Modified);
        Assert.Equal(6, actual.Revision);
    }

    [Fact]
    public async Task UpdatePasswordThrowsIfRecordNotFound()
    {
        using var connection = await TestDatabase.OpenConnectionWithRollback();

        await new SampleDataHelper(connection).InsertUser(ModelFactory.CreateUser(id: 23));

        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => this.userDataProvider.UpdatePassword(
                connection, 4, "new-hashed-password", "newstamp"));

        Assert.Equal("User 4 not found", exception.Message);
    }

    #endregion

    #region UpdatePreferences

    [Fact]
    public async Task UpdatePreferencesUpdatesPreferences()
    {
        using var connection = await TestDatabase.OpenConnectionWithRollback();

        await new SampleDataHelper(connection).InsertUser(
            ModelFactory.CreateUser(id: 32, revision: 2));

        await this.userDataProvider.UpdatePreferences(connection, 32, "new-time-zone");

        var actual = await this.userDataProvider.GetUser(connection, 32);

        Assert.Equal("new-time-zone", actual.TimeZone);
        Assert.Equal(this.fakeTime, actual.Modified);
        Assert.Equal(3, actual.Revision);
    }

    [Fact]
    public async Task UpdatePreferencesThrowsIfRecordNotFound()
    {
        using var connection = await TestDatabase.OpenConnectionWithRollback();

        await new SampleDataHelper(connection).InsertUser(ModelFactory.CreateUser(id: 1));

        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => this.userDataProvider.UpdatePreferences(connection, 9, "new-time-zone"));

        Assert.Equal("User 9 not found", exception.Message);
    }

    #endregion

    #region ReadUser

    [Fact]
    public async Task ReadUserReadsAllAttributes()
    {
        using var connection = await TestDatabase.OpenConnectionWithRollback();

        var expected = await new SampleDataHelper(connection).InsertUser(
            includeOptionalAttributes: true);

        var actual = await this.userDataProvider.GetUser(connection, expected.Id);

        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.Name, actual.Name);
        Assert.Equal(expected.Email, actual.Email);
        Assert.Equal(expected.HashedPassword, actual.HashedPassword);
        Assert.Equal(expected.PasswordCreated, actual.PasswordCreated);
        Assert.Equal(expected.SecurityStamp, actual.SecurityStamp);
        Assert.Equal(expected.TimeZone, actual.TimeZone);
        Assert.Equal(expected.Created, actual.Created);
        Assert.Equal(expected.Modified, actual.Modified);
        Assert.Equal(expected.Revision, actual.Revision);
    }

    [Fact]
    public async Task ReadUserHandlesNullAttributes()
    {
        using var connection = await TestDatabase.OpenConnectionWithRollback();

        var expected = await new SampleDataHelper(connection).InsertUser(
            includeOptionalAttributes: false);

        var actual = await this.userDataProvider.GetUser(connection, expected.Id);

        Assert.Null(actual.HashedPassword);
        Assert.Null(actual.PasswordCreated);
    }

    #endregion
}
