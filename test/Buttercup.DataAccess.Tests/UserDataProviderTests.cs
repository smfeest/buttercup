using System;
using System.Threading.Tasks;
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

            Assert.Equal(4, actual.Id);
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

        #region ReadUser

        [Fact]
        public Task ReadUserReadsAllAttributes() =>
            this.databaseFixture.WithRollback(async connection =>
        {
            var expected = SampleUsers.CreateSampleUser();

            await SampleUsers.InsertSampleUser(connection, expected);

            var actual = await new UserDataProvider().GetUser(connection, expected.Id);

            Assert.Equal(expected.Id, actual.Id);
            Assert.Equal(expected.Email, actual.Email);
            Assert.Equal(expected.HashedPassword, actual.HashedPassword);
            Assert.Equal(expected.Created, actual.Created);
            Assert.Equal(DateTimeKind.Utc, actual.Created.Kind);
            Assert.Equal(expected.Modified, actual.Modified);
            Assert.Equal(DateTimeKind.Utc, actual.Modified.Kind);
            Assert.Equal(expected.Revision, actual.Revision);
        });

        #endregion
    }
}
