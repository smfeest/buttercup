using System.Net.Sockets;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Moq;
using StackExchange.Redis;
using Xunit;

namespace Buttercup.Redis;

public sealed class RedisConnectionManagerTests
{
    private readonly Mock<IRedisConnectionFactory> connectionFactoryMock = new();
    private readonly RedisConnectionOptions options = new()
    {
        ConnectionString = "fake-connection-string",
        DroppedConnectionGracePeriod = TimeSpan.FromSeconds(25),
        DroppedConnectionEpisodeTimeout = TimeSpan.FromSeconds(65),
        MinForcedReconnectionInterval = TimeSpan.FromSeconds(55),
    };
    private readonly FakeTimeProvider timeProvider = new();

    private RedisConnectionManager CreateRedisConnectionManager() =>
        new(this.connectionFactoryMock.Object, Options.Create(this.options), this.timeProvider);

    #region CurrentConnection

    [Fact]
    public async Task CurrentConnection_ThrowsIfStillInitializing()
    {
        var tcs = new TaskCompletionSource<IConnectionMultiplexer>();
        this.connectionFactoryMock.Setup(x => x.NewConnection()).Returns(tcs.Task);

        await using var connectionManager = this.CreateRedisConnectionManager();

        Assert.Throws<InvalidOperationException>(() => connectionManager.CurrentConnection);
    }

    [Fact]
    public async Task CurrentConnection_ReturnsConnectionOnceInitialized()
    {
        var connection = Mock.Of<IConnectionMultiplexer>();
        this.connectionFactoryMock.Setup(x => x.NewConnection()).ReturnsAsync(connection);

        await using var connectionManager = this.CreateRedisConnectionManager();
        await connectionManager.EnsureInitialized();

        Assert.Same(connection, connectionManager.CurrentConnection);
    }

    #endregion

    #region CheckException

    [Fact]
    public async Task CheckException_RedisConnectionException_ReturnsTrue()
    {
        await using var connectionManager = this.CreateRedisConnectionManager();
        await connectionManager.EnsureInitialized();

        Assert.True(await connectionManager.CheckException(
            new RedisConnectionException(ConnectionFailureType.SocketClosed, string.Empty)));
    }

    [Fact]
    public async Task CheckException_SocketException_ReturnsTrue()
    {
        await using var connectionManager = this.CreateRedisConnectionManager();
        await connectionManager.EnsureInitialized();

        Assert.True(await connectionManager.CheckException(new SocketException()));
    }

    [Fact]
    public async Task CheckException_AnyOtherExceptionType_ReturnsFalse()
    {
        await using var connectionManager = this.CreateRedisConnectionManager();
        await connectionManager.EnsureInitialized();

        Assert.False(await connectionManager.CheckException(new InvalidDataException()));
    }

    [Fact]
    public async Task CheckException_MinForcedReconnectionIntervalNotElapsedSinceInitialization_DoesNotReconnect()
    {
        var initialConnection = Mock.Of<IConnectionMultiplexer>();
        this.connectionFactoryMock.Setup(x => x.NewConnection()).ReturnsAsync(initialConnection);

        await using var connectionManager = this.CreateRedisConnectionManager();
        await connectionManager.EnsureInitialized();

        this.timeProvider.Advance(TimeSpan.FromSeconds(5));
        await connectionManager.CheckException(new SocketException()); // First error

        this.timeProvider.Advance(this.options.MinForcedReconnectionInterval.Subtract(TimeSpan.FromSeconds(6)));
        await connectionManager.CheckException(new SocketException()); // Second error

        this.connectionFactoryMock.Verify(x => x.NewConnection(), Times.Once);
        Assert.Same(initialConnection, connectionManager.CurrentConnection);
    }

    [Fact]
    public async Task CheckException_MinForcedReconnectionIntervalNotElapsedSinceReconnection_DoesNotReconnect()
    {
        var secondConnection = Mock.Of<IConnectionMultiplexer>();
        this.connectionFactoryMock.SetupSequence(x => x.NewConnection())
            .ReturnsAsync(Mock.Of<IConnectionMultiplexer>())
            .ReturnsAsync(secondConnection);

        await using var connectionManager = this.CreateRedisConnectionManager();
        await connectionManager.EnsureInitialized();

        this.timeProvider.Advance(this.options.MinForcedReconnectionInterval.Add(TimeSpan.FromSeconds(1)));
        await connectionManager.CheckException(new SocketException()); // First error since initialization

        this.timeProvider.Advance(this.options.DroppedConnectionGracePeriod.Add(TimeSpan.FromSeconds(1)));
        await connectionManager.CheckException(new SocketException()); // Second error since initialization (reconnection)

        this.timeProvider.Advance(TimeSpan.FromSeconds(5));
        await connectionManager.CheckException(new SocketException()); // First error since reconnection

        this.timeProvider.Advance(this.options.MinForcedReconnectionInterval.Subtract(TimeSpan.FromSeconds(6)));
        await connectionManager.CheckException(new SocketException()); // Second error since reconnection

        this.connectionFactoryMock.Verify(x => x.NewConnection(), Times.Exactly(2));
        Assert.Same(secondConnection, connectionManager.CurrentConnection);
    }

    [Fact]
    public async Task CheckException_DroppedConnectionGracePeriodNotElapsedSinceFirstErrorAfterInitialization_DoesNotReconnect()
    {
        var initialConnection = Mock.Of<IConnectionMultiplexer>();
        this.connectionFactoryMock.Setup(x => x.NewConnection()).ReturnsAsync(initialConnection);

        await using var connectionManager = this.CreateRedisConnectionManager();
        await connectionManager.EnsureInitialized();

        this.timeProvider.Advance(this.options.MinForcedReconnectionInterval.Add(TimeSpan.FromSeconds(1)));
        await connectionManager.CheckException(new SocketException()); // First error

        this.timeProvider.Advance(this.options.DroppedConnectionGracePeriod.Subtract(TimeSpan.FromSeconds(1)));
        await connectionManager.CheckException(new SocketException()); // Second error

        this.connectionFactoryMock.Verify(x => x.NewConnection(), Times.Once);
        Assert.Same(initialConnection, connectionManager.CurrentConnection);
    }

    [Fact]
    public async Task CheckException_DroppedConnectionGracePeriodNotElapsedSinceFirstErrorAfterReconnection_DoesNotReconnect()
    {
        var secondConnection = Mock.Of<IConnectionMultiplexer>();
        this.connectionFactoryMock.SetupSequence(x => x.NewConnection())
            .ReturnsAsync(Mock.Of<IConnectionMultiplexer>())
            .ReturnsAsync(secondConnection);

        await using var connectionManager = this.CreateRedisConnectionManager();
        await connectionManager.EnsureInitialized();

        this.timeProvider.Advance(this.options.MinForcedReconnectionInterval.Add(TimeSpan.FromSeconds(1)));
        await connectionManager.CheckException(new SocketException()); // First error since initialization

        this.timeProvider.Advance(this.options.DroppedConnectionGracePeriod.Add(TimeSpan.FromSeconds(1)));
        await connectionManager.CheckException(new SocketException()); // Second error since initialization (reconnection)

        this.timeProvider.Advance(this.options.MinForcedReconnectionInterval.Add(TimeSpan.FromSeconds(1)));
        await connectionManager.CheckException(new SocketException()); // First error since reconnection

        this.timeProvider.Advance(this.options.DroppedConnectionGracePeriod.Subtract(TimeSpan.FromSeconds(1)));
        await connectionManager.CheckException(new SocketException()); // Second error since reconnection

        this.connectionFactoryMock.Verify(x => x.NewConnection(), Times.Exactly(2));
        Assert.Same(secondConnection, connectionManager.CurrentConnection);
    }

    [Fact]
    public async Task CheckException_DroppedConnectionEpisodeTimeoutElapsedSinceMostRecentErrorAfterInitialization_DoesNotReconnect()
    {
        var initialConnection = Mock.Of<IConnectionMultiplexer>();
        this.connectionFactoryMock.Setup(x => x.NewConnection()).ReturnsAsync(initialConnection);

        await using var connectionManager = this.CreateRedisConnectionManager();
        await connectionManager.EnsureInitialized();

        this.timeProvider.Advance(this.options.MinForcedReconnectionInterval.Add(TimeSpan.FromSeconds(1)));
        await connectionManager.CheckException(new SocketException()); // First error

        this.timeProvider.Advance(this.options.DroppedConnectionEpisodeTimeout.Add(TimeSpan.FromSeconds(1)));
        await connectionManager.CheckException(new SocketException()); // Second error

        this.connectionFactoryMock.Verify(x => x.NewConnection(), Times.Once);
        Assert.Same(initialConnection, connectionManager.CurrentConnection);
    }

    [Fact]
    public async Task CheckException_DroppedConnectionGracePeriodElapsedOnlyIfMeasuredFromFirstError_Reconnects()
    {
        var newConnection = Mock.Of<IConnectionMultiplexer>();
        this.connectionFactoryMock.SetupSequence(x => x.NewConnection())
            .ReturnsAsync(Mock.Of<IConnectionMultiplexer>())
            .ReturnsAsync(newConnection);

        await using var connectionManager = this.CreateRedisConnectionManager();
        await connectionManager.EnsureInitialized();

        this.timeProvider.Advance(this.options.MinForcedReconnectionInterval.Add(TimeSpan.FromSeconds(1)));
        await connectionManager.CheckException(new SocketException()); // First error

        this.timeProvider.Advance(this.options.DroppedConnectionGracePeriod.Subtract(TimeSpan.FromSeconds(1)));
        await connectionManager.CheckException(new SocketException()); // Second error

        this.timeProvider.Advance(TimeSpan.FromSeconds(2));
        await connectionManager.CheckException(new SocketException()); // Third error

        Assert.Same(newConnection, connectionManager.CurrentConnection);
    }

    [Fact]
    public async Task CheckException_DroppedConnectionEpisodeTimeoutNotElapsedOnlyIfMeasuredFromPreviousError_Reconnects()
    {
        var newConnection = Mock.Of<IConnectionMultiplexer>();
        this.connectionFactoryMock.SetupSequence(x => x.NewConnection())
            .ReturnsAsync(Mock.Of<IConnectionMultiplexer>())
            .ReturnsAsync(newConnection);

        await using var connectionManager = this.CreateRedisConnectionManager();
        await connectionManager.EnsureInitialized();
        var initialConnection = connectionManager.CurrentConnection;

        this.timeProvider.Advance(this.options.MinForcedReconnectionInterval.Add(TimeSpan.FromSeconds(1)));
        await connectionManager.CheckException(new SocketException()); // First error

        this.timeProvider.Advance(this.options.DroppedConnectionGracePeriod.Subtract(TimeSpan.FromSeconds(1)));
        await connectionManager.CheckException(new SocketException()); // Second error

        this.timeProvider.Advance(this.options.DroppedConnectionEpisodeTimeout.Subtract(TimeSpan.FromSeconds(1)));
        await connectionManager.CheckException(new SocketException()); // Third error

        Assert.Same(newConnection, connectionManager.CurrentConnection);
    }

    [Fact]
    public async Task CheckException_DisposesOldConnectionOnReconnection()
    {
        var initialConnectionMock = new Mock<IConnectionMultiplexer>();
        this.connectionFactoryMock.SetupSequence(x => x.NewConnection())
            .ReturnsAsync(initialConnectionMock.Object)
            .ReturnsAsync(Mock.Of<IConnectionMultiplexer>());

        await using var connectionManager = this.CreateRedisConnectionManager();
        await connectionManager.EnsureInitialized();

        this.timeProvider.Advance(this.options.MinForcedReconnectionInterval.Add(TimeSpan.FromSeconds(1)));
        await connectionManager.CheckException(new SocketException()); // First error

        this.timeProvider.Advance(this.options.DroppedConnectionGracePeriod.Add(TimeSpan.FromSeconds(1)));
        await connectionManager.CheckException(new SocketException()); // Second error (reconnection)

        initialConnectionMock.Verify(x => x.DisposeAsync());
    }

    [Fact]
    public async Task CheckException_DisposedDuringReconnection_DisposesNewConnection()
    {
        var newConnectionTcs = new TaskCompletionSource<IConnectionMultiplexer>();
        this.connectionFactoryMock.Setup(x => x.NewConnection()).Returns(newConnectionTcs.Task);

        this.connectionFactoryMock.SetupSequence(x => x.NewConnection())
            .ReturnsAsync(Mock.Of<IConnectionMultiplexer>())
            .Returns(newConnectionTcs.Task);

        await using (var connectionManager = this.CreateRedisConnectionManager())
        {
            await connectionManager.EnsureInitialized();

            this.timeProvider.Advance(this.options.MinForcedReconnectionInterval.Add(TimeSpan.FromSeconds(1)));
            await connectionManager.CheckException(new SocketException()); // First error

            this.timeProvider.Advance(this.options.DroppedConnectionGracePeriod.Add(TimeSpan.FromSeconds(1)));
            _ = connectionManager.CheckException(new SocketException()); // Second error (reconnection)
        }

        var newConnectionMock = new Mock<IConnectionMultiplexer>();
        newConnectionTcs.SetResult(newConnectionMock.Object);

        newConnectionMock.Verify(x => x.DisposeAsync());
    }

    #endregion

    #region DisposeAsync

    [Fact]
    public async Task DisposeAsync_DisposesCurrentConnection()
    {
        var connectionMock = new Mock<IConnectionMultiplexer>();
        this.connectionFactoryMock
            .Setup(x => x.NewConnection())
            .ReturnsAsync(connectionMock.Object);

        await using (var connectionManager = this.CreateRedisConnectionManager())
        {
            await connectionManager.EnsureInitialized();
        }

        connectionMock.Verify(x => x.DisposeAsync());
    }

    [Fact]
    public async Task DisposeAsync_SafeToCallWhileInitializing()
    {
        var tcs = new TaskCompletionSource<IConnectionMultiplexer>();
        this.connectionFactoryMock.Setup(x => x.NewConnection()).Returns(tcs.Task);

        await using (var connectionManager = this.CreateRedisConnectionManager())
        {
        }

        var connectionMock = new Mock<IConnectionMultiplexer>();
        tcs.SetResult(connectionMock.Object);

        connectionMock.Verify(x => x.DisposeAsync());
    }

    #endregion

    #region EnsureInitialized

    [Fact]
    public async Task EnsureInitialized_ReturnsSameTaskOnEveryCall()
    {
        await using var connectionManager = this.CreateRedisConnectionManager();

        Assert.Same(
            connectionManager.EnsureInitialized(),
            connectionManager.EnsureInitialized());

        await connectionManager.EnsureInitialized();
    }

    #endregion
}
