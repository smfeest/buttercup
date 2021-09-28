using System;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace Buttercup.DataAccess
{
    [Collection("Database collection")]
    public class UserDataProviderTests
    {
        private readonly DatabaseFixture databaseFixture;

        public UserDataProviderTests(DatabaseFixture databaseFixture) =>
            this.databaseFixture = databaseFixture;

        #region FindUserByEmail

        [Fact]
        public async Task FindUserByEmailReturnsUser() =>
            await this.databaseFixture.WithRollback(async connection =>
        {
            await SampleUsers.InsertSampleUser(
                connection, SampleUsers.CreateSampleUser(id: 4, email: "alpha@example.com"));

            var actual = await new UserDataProvider().FindUserByEmail(
                connection, "alpha@example.com");

            Assert.Equal(4, actual!.Id);
            Assert.Equal("alpha@example.com", actual.Email);
        });

        [Fact]
        public async Task FindUserByEmailReturnsNullIfNoMatchFound() =>
            await this.databaseFixture.WithRollback(async connection =>
        {
            await SampleUsers.InsertSampleUser(
                connection, SampleUsers.CreateSampleUser(email: "alpha@example.com"));

            var actual = await new UserDataProvider().FindUserByEmail(
                connection, "beta@example.com");

            Assert.Null(actual);
        });

        #endregion

        #region GetUser

        [Fact]
        public async Task GetUserReturnsUser() =>
            await this.databaseFixture.WithRollback(async connection =>
        {
            var expected = SampleUsers.CreateSampleUser(id: 76);

            await SampleUsers.InsertSampleUser(connection, expected);

            var actual = await new UserDataProvider().GetUser(connection, 76);

            Assert.Equal(76, actual.Id);
            Assert.Equal(expected.Email, actual.Email);
        });

        [Fact]
        public async Task GetUserThrowsIfRecordNotFound() =>
            await this.databaseFixture.WithRollback(async connection =>
        {
            await SampleUsers.InsertSampleUser(connection, SampleUsers.CreateSampleUser(id: 98));

            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => new UserDataProvider().GetUser(connection, 7));

            Assert.Equal("User 7 not found", exception.Message);
        });

        #endregion

        #region UpdatePassword

        [Fact]
        public Task UpdatePasswordUpdatesHashedPassword() =>
            this.databaseFixture.WithRollback(async connection =>
        {
            var userDataProvider = new UserDataProvider();

            await SampleUsers.InsertSampleUser(
                connection, SampleUsers.CreateSampleUser(id: 41, revision: 5));

            var time = new DateTime(2003, 4, 5, 6, 7, 8);

            await userDataProvider.UpdatePassword(
                connection, 41, "new-hashed-password", "newstamp", time);

            var actual = await userDataProvider.GetUser(connection, 41);

            Assert.Equal("new-hashed-password", actual.HashedPassword);
            Assert.Equal(time, actual.PasswordCreated);
            Assert.Equal("newstamp", actual.SecurityStamp);
            Assert.Equal(time, actual.Modified);
            Assert.Equal(6, actual.Revision);
        });

        [Fact]
        public async Task UpdatePasswordThrowsIfRecordNotFound() =>
            await this.databaseFixture.WithRollback(async connection =>
        {
            await SampleUsers.InsertSampleUser(connection, SampleUsers.CreateSampleUser(id: 23));

            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => new UserDataProvider().UpdatePassword(
                    connection, 4, "new-hashed-password", "newstamp", DateTime.UtcNow));

            Assert.Equal("User 4 not found", exception.Message);
        });

        #endregion

        #region UpdatePreferences

        [Fact]
        public Task UpdatePreferencesUpdatesPreferences() =>
            this.databaseFixture.WithRollback(async connection =>
        {
            var userDataProvider = new UserDataProvider();

            await SampleUsers.InsertSampleUser(
                connection, SampleUsers.CreateSampleUser(id: 32, revision: 2));

            var time = new DateTime(2003, 4, 5, 6, 7, 8);

            await userDataProvider.UpdatePreferences(connection, 32, "new-time-zone", time);

            var actual = await userDataProvider.GetUser(connection, 32);

            Assert.Equal("new-time-zone", actual.TimeZone);
            Assert.Equal(time, actual.Modified);
            Assert.Equal(3, actual.Revision);
        });

        [Fact]
        public async Task UpdatePreferencesThrowsIfRecordNotFound() =>
            await this.databaseFixture.WithRollback(async connection =>
        {
            await SampleUsers.InsertSampleUser(connection, SampleUsers.CreateSampleUser(id: 1));

            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => new UserDataProvider().UpdatePreferences(
                    connection, 9, "new-time-zone", DateTime.UtcNow));

            Assert.Equal("User 9 not found", exception.Message);
        });

        #endregion

        #region ReadUser

        [Fact]
        public Task ReadUserReadsAllAttributes() =>
            this.databaseFixture.WithRollback(async connection =>
        {
            var expected = SampleUsers.CreateSampleUser();

            await SampleUsers.InsertSampleUser(connection, expected);

            var actual = await new UserDataProvider().GetUser(connection, expected.Id);

            Assert.Equal(expected.Id, actual.Id);
            Assert.Equal(expected.Name, actual.Name);
            Assert.Equal(expected.Email, actual.Email);
            Assert.Equal(expected.HashedPassword, actual.HashedPassword);
            Assert.Equal(expected.PasswordCreated, actual.PasswordCreated);
            Assert.Equal(DateTimeKind.Utc, actual.PasswordCreated!.Value.Kind);
            Assert.Equal(expected.SecurityStamp, actual.SecurityStamp);
            Assert.Equal(expected.TimeZone, actual.TimeZone);
            Assert.Equal(expected.Created, actual.Created);
            Assert.Equal(DateTimeKind.Utc, actual.Created.Kind);
            Assert.Equal(expected.Modified, actual.Modified);
            Assert.Equal(DateTimeKind.Utc, actual.Modified.Kind);
            Assert.Equal(expected.Revision, actual.Revision);
        });

        #endregion
    }
}
