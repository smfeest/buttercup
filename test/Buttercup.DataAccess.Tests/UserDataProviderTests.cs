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

        #region ReadUser

        [Fact]
        public Task ReadUserReadsAllAttributes() =>
            this.databaseFixture.WithRollback(async connection =>
        {
            var expected = SampleUsers.CreateSampleUser();

            await SampleUsers.InsertSampleUser(connection, expected);

            var actual = await new UserDataProvider().FindUserByEmail(connection, expected.Email);

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
