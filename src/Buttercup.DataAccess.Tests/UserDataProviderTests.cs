using System;
using System.Threading.Tasks;
using Xunit;

namespace Buttercup.DataAccess
{
    [Collection("Database collection")]
    public class UserDataProviderTests
    {
        #region FindUserByEmail

        [Fact]
        public async Task FindUserByEmailReturnsUser()
        {
            using var connection = await TestDatabase.OpenConnectionWithRollback();

            await SampleUsers.InsertSampleUser(
                connection, SampleUsers.CreateSampleUser(id: 4, email: "alpha@example.com"));

            var actual = await new UserDataProvider().FindUserByEmail(
                connection, "alpha@example.com");

            Assert.Equal(4, actual!.Id);
            Assert.Equal("alpha@example.com", actual.Email);
        }

        [Fact]
        public async Task FindUserByEmailReturnsNullIfNoMatchFound()
        {
            using var connection = await TestDatabase.OpenConnectionWithRollback();

            await SampleUsers.InsertSampleUser(
                connection, SampleUsers.CreateSampleUser(email: "alpha@example.com"));

            var actual = await new UserDataProvider().FindUserByEmail(
                connection, "beta@example.com");

            Assert.Null(actual);
        }

        #endregion

        #region GetUser

        [Fact]
        public async Task GetUserReturnsUser()
        {
            using var connection = await TestDatabase.OpenConnectionWithRollback();

            var expected = SampleUsers.CreateSampleUser(id: 76);

            await SampleUsers.InsertSampleUser(connection, expected);

            var actual = await new UserDataProvider().GetUser(connection, 76);

            Assert.Equal(76, actual.Id);
            Assert.Equal(expected.Email, actual.Email);
        }

        [Fact]
        public async Task GetUserThrowsIfRecordNotFound()
        {
            using var connection = await TestDatabase.OpenConnectionWithRollback();

            await SampleUsers.InsertSampleUser(connection, SampleUsers.CreateSampleUser(id: 98));

            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => new UserDataProvider().GetUser(connection, 7));

            Assert.Equal("User 7 not found", exception.Message);
        }

        #endregion

        #region UpdatePassword

        [Fact]
        public async Task UpdatePasswordUpdatesHashedPassword()
        {
            using var connection = await TestDatabase.OpenConnectionWithRollback();

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
        }

        [Fact]
        public async Task UpdatePasswordThrowsIfRecordNotFound()
        {
            using var connection = await TestDatabase.OpenConnectionWithRollback();

            await SampleUsers.InsertSampleUser(connection, SampleUsers.CreateSampleUser(id: 23));

            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => new UserDataProvider().UpdatePassword(
                    connection, 4, "new-hashed-password", "newstamp", DateTime.UtcNow));

            Assert.Equal("User 4 not found", exception.Message);
        }

        #endregion

        #region UpdatePreferences

        [Fact]
        public async Task UpdatePreferencesUpdatesPreferences()
        {
            using var connection = await TestDatabase.OpenConnectionWithRollback();

            var userDataProvider = new UserDataProvider();

            await SampleUsers.InsertSampleUser(
                connection, SampleUsers.CreateSampleUser(id: 32, revision: 2));

            var time = new DateTime(2003, 4, 5, 6, 7, 8);

            await userDataProvider.UpdatePreferences(connection, 32, "new-time-zone", time);

            var actual = await userDataProvider.GetUser(connection, 32);

            Assert.Equal("new-time-zone", actual.TimeZone);
            Assert.Equal(time, actual.Modified);
            Assert.Equal(3, actual.Revision);
        }

        [Fact]
        public async Task UpdatePreferencesThrowsIfRecordNotFound()
        {
            using var connection = await TestDatabase.OpenConnectionWithRollback();

            await SampleUsers.InsertSampleUser(connection, SampleUsers.CreateSampleUser(id: 1));

            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => new UserDataProvider().UpdatePreferences(
                    connection, 9, "new-time-zone", DateTime.UtcNow));

            Assert.Equal("User 9 not found", exception.Message);
        }

        #endregion

        #region ReadUser

        [Fact]
        public async Task ReadUserReadsAllAttributes()
        {
            using var connection = await TestDatabase.OpenConnectionWithRollback();

            var expected = SampleUsers.CreateSampleUser(includeOptionalAttributes: true);

            await SampleUsers.InsertSampleUser(connection, expected);

            var actual = await new UserDataProvider().GetUser(connection, expected.Id);

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

            var expected = SampleUsers.CreateSampleUser(includeOptionalAttributes: false);

            await SampleUsers.InsertSampleUser(connection, expected);

            var actual = await new UserDataProvider().GetUser(connection, expected.Id);

            Assert.Null(actual.HashedPassword);
            Assert.Null(actual.PasswordCreated);
        }

        #endregion
    }
}
