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

            var actual = await new Context().UserDataProvider.FindUserByEmail(
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

            var actual = await new Context().UserDataProvider.FindUserByEmail(
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

            var actual = await new Context().UserDataProvider.GetUser(connection, 76);

            Assert.Equal(76, actual.Id);
            Assert.Equal(expected.Email, actual.Email);
        });

        [Fact]
        public async Task GetUserThrowsIfRecordNotFound() =>
            await this.databaseFixture.WithRollback(async connection =>
        {
            await SampleUsers.InsertSampleUser(connection, SampleUsers.CreateSampleUser(id: 98));

            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => new Context().UserDataProvider.GetUser(connection, 7));

            Assert.Equal("User 7 not found", exception.Message);
        });

        #endregion

        #region UpdatePassword

        [Fact]
        public Task UpdatePasswordUpdatesHashedPassword() =>
            this.databaseFixture.WithRollback(async connection =>
        {
            var context = new Context();

            await SampleUsers.InsertSampleUser(
                connection, SampleUsers.CreateSampleUser(id: 41, revision: 5));

            var utcNow = new DateTime(2003, 4, 5, 6, 7, 8);
            context.SetupUtcNow(utcNow);

            await context.UserDataProvider.UpdatePassword(connection, 41, "new-hashed-password");

            var actual = await context.UserDataProvider.GetUser(connection, 41);

            Assert.Equal("new-hashed-password", actual.HashedPassword);
            Assert.Equal(utcNow, actual.Modified);
            Assert.Equal(6, actual.Revision);
        });

        [Fact]
        public async Task UpdatePasswordThrowsIfRecordNotFound() =>
            await this.databaseFixture.WithRollback(async connection =>
        {
            await SampleUsers.InsertSampleUser(connection, SampleUsers.CreateSampleUser(id: 23));

            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => new Context().UserDataProvider.UpdatePassword(
                    connection, 4, "new-hashed-password"));

            Assert.Equal("User 4 not found", exception.Message);
        });

        #endregion

        #region ReadUser

        [Fact]
        public Task ReadUserReadsAllAttributes() =>
            this.databaseFixture.WithRollback(async connection =>
        {
            var expected = SampleUsers.CreateSampleUser();

            await SampleUsers.InsertSampleUser(connection, expected);

            var actual = await new Context().UserDataProvider.GetUser(connection, expected.Id);

            Assert.Equal(expected.Id, actual.Id);
            Assert.Equal(expected.Email, actual.Email);
            Assert.Equal(expected.HashedPassword, actual.HashedPassword);
            Assert.Equal(expected.SecurityStamp, actual.SecurityStamp);
            Assert.Equal(expected.Created, actual.Created);
            Assert.Equal(DateTimeKind.Utc, actual.Created.Kind);
            Assert.Equal(expected.Modified, actual.Modified);
            Assert.Equal(DateTimeKind.Utc, actual.Modified.Kind);
            Assert.Equal(expected.Revision, actual.Revision);
        });

        #endregion

        private class Context
        {
            public Context() =>
                this.UserDataProvider = new UserDataProvider(this.MockClock.Object);

            public UserDataProvider UserDataProvider { get; }

            public Mock<IClock> MockClock { get; } = new Mock<IClock>();

            public void SetupUtcNow(DateTime utcNow) =>
                this.MockClock.SetupGet(x => x.UtcNow).Returns(utcNow);
        }
    }
}
