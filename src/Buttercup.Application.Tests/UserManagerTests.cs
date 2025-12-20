using Buttercup.EntityModel;
using Buttercup.TestUtils;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
using Moq;
using Xunit;

namespace Buttercup.Application;

[Collection(nameof(DatabaseCollection))]
public sealed class UserManagerTests : DatabaseTests<DatabaseCollection>
{
    private readonly ModelFactory modelFactory = new();

    private readonly Mock<IPasswordHasher<User>> passwordHasherMock = new();
    private readonly Mock<IRandomTokenGenerator> randomTokenGeneratorMock = new();
    private readonly FakeTimeProvider timeProvider;
    private readonly UserManager userManager;

    public UserManagerTests(DatabaseFixture<DatabaseCollection> databaseFixture)
        : base(databaseFixture)
    {
        this.timeProvider = new(this.modelFactory.NextDateTime());
        this.userManager = new(
            this.DatabaseFixture,
            this.passwordHasherMock.Object,
            this.randomTokenGeneratorMock.Object,
            this.timeProvider);
    }

    #region CreateUser

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CreateUser_Success(bool isAdmin)
    {
        var currentUser = this.modelFactory.BuildUser();
        await this.DatabaseFixture.InsertEntities(currentUser);

        var securityStamp = this.modelFactory.NextToken(8);
        this.randomTokenGeneratorMock.Setup(x => x.Generate(2)).Returns(securityStamp);

        var attributes = new NewUserAttributes
        {
            Name = this.modelFactory.NextString("name"),
            Email = this.modelFactory.NextEmail(),
            TimeZone = this.modelFactory.NextString("time-zone"),
            IsAdmin = isAdmin,
        };
        var ipAddress = this.modelFactory.NextIpAddress();

        var id = await this.userManager.CreateUser(attributes, currentUser.Id, ipAddress);

        using var dbContext = this.DatabaseFixture.CreateDbContext();

        // Inserts user
        var expected = new User
        {
            Id = id,
            Name = attributes.Name,
            Email = attributes.Email,
            HashedPassword = null,
            PasswordCreated = null,
            SecurityStamp = securityStamp,
            TimeZone = attributes.TimeZone,
            IsAdmin = attributes.IsAdmin,
            Created = this.timeProvider.GetUtcDateTimeNow(),
            Modified = this.timeProvider.GetUtcDateTimeNow(),
            Revision = 0,
        };
        var actual = await dbContext.Users.FindAsync([id], TestContext.Current.CancellationToken);
        Assert.Equal(expected, actual);

        // Inserts user audit entry
        var userAuditEntry = await dbContext.UserAuditEntries.SingleAsync(
            TestContext.Current.CancellationToken);
        Assert.Equal(this.timeProvider.GetUtcDateTimeNow(), userAuditEntry.Time);
        Assert.Equal(UserOperation.Create, userAuditEntry.Operation);
        Assert.Equal(id, userAuditEntry.TargetId);
        Assert.Equal(currentUser.Id, userAuditEntry.ActorId);
        Assert.Equal(ipAddress, userAuditEntry.IpAddress);
    }

    [Fact]
    public async Task CreateUser_EmailNotUnique()
    {
        var existing = this.modelFactory.BuildUser();
        await this.DatabaseFixture.InsertEntities(existing);

        var attributes = new NewUserAttributes
        {
            Name = this.modelFactory.NextString("name"),
            Email = existing.Email,
            TimeZone = this.modelFactory.NextString("time-zone"),
        };

        var exception = await Assert.ThrowsAsync<NotUniqueException>(
            () => this.userManager.CreateUser(attributes, existing.Id, null));
        Assert.Equal(nameof(attributes.Email), exception.PropertyName);
        Assert.Equal(
            $"Another user already exists with email '{attributes.Email}'",
            exception.Message);
    }

    #endregion

    #region CreateTestUser

    [Fact]
    public async Task CreateTestUser()
    {
        var suffix = this.modelFactory.NextString("suffix");
        var securityStamp = this.modelFactory.NextToken(8);
        this.randomTokenGeneratorMock.SetupSequence(x => x.Generate(2))
            .Returns(suffix)
            .Returns(securityStamp);

        var password = this.modelFactory.NextString("password");
        this.randomTokenGeneratorMock.Setup(x => x.Generate(4)).Returns(password);

        var hashedPassword = this.modelFactory.NextString("hashed-password");
        this.passwordHasherMock
            .Setup(x => x.HashPassword(It.IsAny<User>(), password))
            .Returns(hashedPassword);

        var (id, returnedPassword) = await this.userManager.CreateTestUser();

        Assert.Equal(password, returnedPassword);

        var expected = new User
        {
            Id = id,
            Name = $"Test User {suffix}",
            Email = $"test+{suffix}@example.com",
            HashedPassword = hashedPassword,
            PasswordCreated = this.timeProvider.GetUtcDateTimeNow(),
            SecurityStamp = securityStamp,
            TimeZone = "Etc/UTC",
            IsAdmin = false,
            Created = this.timeProvider.GetUtcDateTimeNow(),
            Modified = this.timeProvider.GetUtcDateTimeNow(),
            Revision = 0,
        };
        var actual = await this.userManager.FindUser(id);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task CreateTestUser_RetriesIfEmailNotUnique()
    {
        var firstSuffix = this.modelFactory.NextString("suffix");
        var secondSuffix = this.modelFactory.NextString("suffix");
        var firstSecurityStamp = this.modelFactory.NextToken(8);
        var secondSecurityStamp = this.modelFactory.NextToken(8);
        this.randomTokenGeneratorMock.SetupSequence(x => x.Generate(2))
            .Returns(firstSuffix)
            .Returns(firstSecurityStamp)
            .Returns(secondSuffix)
            .Returns(secondSecurityStamp);

        var password = this.modelFactory.NextString("password");
        this.randomTokenGeneratorMock.Setup(x => x.Generate(4)).Returns(password);

        var hashedPassword = this.modelFactory.NextString("hashed-password");
        this.passwordHasherMock
            .Setup(x => x.HashPassword(It.IsAny<User>(), password))
            .Returns(hashedPassword);

        await this.DatabaseFixture.InsertEntities(
            this.modelFactory.BuildUser() with { Email = $"test+{firstSuffix}@example.com" });

        var (id, returnedPassword) = await this.userManager.CreateTestUser();

        Assert.Equal(password, returnedPassword);

        var expected = new User
        {
            Id = id,
            Name = $"Test User {secondSuffix}",
            Email = $"test+{secondSuffix}@example.com",
            HashedPassword = hashedPassword,
            PasswordCreated = this.timeProvider.GetUtcDateTimeNow(),
            SecurityStamp = secondSecurityStamp,
            TimeZone = "Etc/UTC",
            IsAdmin = false,
            Created = this.timeProvider.GetUtcDateTimeNow(),
            Modified = this.timeProvider.GetUtcDateTimeNow(),
            Revision = 0,
        };
        var actual = await this.userManager.FindUser(id);

        Assert.Equal(expected, actual);
    }

    #endregion

    #region FindUser

    [Fact]
    public async Task FindUser_ReturnsUser()
    {
        var expected = this.modelFactory.BuildUser();
        await this.DatabaseFixture.InsertEntities(expected);

        // Returns user
        Assert.Equal(expected, await this.userManager.FindUser(expected.Id));
    }

    [Fact]
    public async Task FindUser_UserDoesNotExist()
    {
        await this.DatabaseFixture.InsertEntities(this.modelFactory.BuildUser());

        // Returns null
        Assert.Null(await this.userManager.FindUser(this.modelFactory.NextInt()));
    }

    #endregion

    #region HardDeleteTestUser

    [Fact]
    public async Task HardDeleteTestUser_UserExists()
    {
        var user = this.modelFactory.BuildUser();
        var otherUser = this.modelFactory.BuildUser();
        var passwordResetTokenForUser = this.modelFactory.BuildPasswordResetToken(user);
        var passwordResetTokenForOtherUser = this.modelFactory.BuildPasswordResetToken(otherUser);
        var securityEventForUser = this.modelFactory.BuildSecurityEvent(user);
        var securityEventForOtherUser = this.modelFactory.BuildSecurityEvent(otherUser);

        await this.DatabaseFixture.InsertEntities(
            user,
            otherUser,
            passwordResetTokenForUser,
            passwordResetTokenForOtherUser,
            securityEventForUser,
            securityEventForOtherUser);

        Assert.True(await this.userManager.HardDeleteTestUser(user.Id));

        using var dbContext = this.DatabaseFixture.CreateDbContext();

        Assert.Equal(
            otherUser,
            await dbContext.Users.SingleAsync(TestContext.Current.CancellationToken));
        Assert.Equal(
            passwordResetTokenForOtherUser.Token,
            await dbContext
                .PasswordResetTokens
                .Select(t => t.Token)
                .SingleAsync(TestContext.Current.CancellationToken));
        Assert.Equal(
            securityEventForOtherUser.Id,
            await dbContext
                .SecurityEvents
                .Select(e => e.Id)
                .SingleAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task HardDeleteTestUser_UserDoesNotExist()
    {
        await this.DatabaseFixture.InsertEntities(this.modelFactory.BuildUser());

        Assert.False(await this.userManager.HardDeleteTestUser(this.modelFactory.NextInt()));
    }

    #endregion

    #region SetTimeZone

    [Fact]
    public async Task SetTimeZone_Success()
    {
        var original = this.modelFactory.BuildUser();
        await this.DatabaseFixture.InsertEntities(original);

        var newTimeZone = this.modelFactory.NextString("new-time-zone");
        await this.userManager.SetTimeZone(original.Id, newTimeZone);

        using var dbContext = this.DatabaseFixture.CreateDbContext();

        // Updates user
        var expected = original with
        {
            TimeZone = newTimeZone,
            Modified = this.timeProvider.GetUtcDateTimeNow(),
            Revision = original.Revision + 1,
        };
        var actual = await dbContext.Users.FindAsync(
            [original.Id], TestContext.Current.CancellationToken);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task SetTimeZone_UserDoesNotExist()
    {
        await this.DatabaseFixture.InsertEntities(this.modelFactory.BuildUser());
        var id = this.modelFactory.NextInt();

        // Throws exception
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => this.userManager.SetTimeZone(id, this.modelFactory.NextString("time-zone")));
        Assert.Equal($"User {id} not found", exception.Message);
    }

    #endregion
}
