using System.Net.Sockets;
using Buttercup.TestUtils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Moq;
using StackExchange.Redis;
using Xunit;

namespace Buttercup.Redis;

public sealed class RedisConnectionManagerTests
{
    private readonly Mock<IRedisConnectionFactory> connectionFactoryMock = new();
    private readonly FakeLogger<RedisConnectionManager> logger = new();
    private readonly RedisConnectionOptions options = new()
    {
        ConnectionString = "fake-connection-string",
        DroppedConnectionGracePeriod = TimeSpan.FromSeconds(25),
        DroppedConnectionEpisodeTimeout = TimeSpan.FromSeconds(65),
        MinForcedReconnectionInterval = TimeSpan.FromSeconds(55),
    };
    private readonly FakeTimeProvider timeProvider = new();

    private RedisConnectionManager CreateRedisConnectionManager() =>
        new(this.connectionFactoryMock.Object,
        this.logger,
        Options.Create(this.options),
        this.timeProvider);

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

        this.connectionFactoryMock.Invocations.Clear();
        this.logger.Collector.Clear();

        this.timeProvider.Advance(this.options.MinForcedReconnectionInterval.Subtract(TimeSpan.FromSeconds(6)));
        await connectionManager.CheckException(new SocketException()); // Second error

        this.connectionFactoryMock.Verify(x => x.NewConnection(), Times.Never);
        Assert.Same(initialConnection, connectionManager.CurrentConnection);

        LogAssert.SingleEntry(this.logger)
            .HasId(3)
            .HasLevel(LogLevel.Debug)
            .HasMessage("No forced reconnection; insufficient time has elapsed since last reconnection");
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

        this.connectionFactoryMock.Invocations.Clear();
        this.logger.Collector.Clear();

        this.timeProvider.Advance(this.options.MinForcedReconnectionInterval.Subtract(TimeSpan.FromSeconds(6)));
        await connectionManager.CheckException(new SocketException()); // Second error since reconnection

        this.connectionFactoryMock.Verify(x => x.NewConnection(), Times.Never);
        Assert.Same(secondConnection, connectionManager.CurrentConnection);

        LogAssert.SingleEntry(this.logger)
            .HasId(3)
            .HasLevel(LogLevel.Debug)
            .HasMessage("No forced reconnection; insufficient time has elapsed since last reconnection");
    }

    [Fact]
    public async Task CheckException_FirstErrorSinceInitialization_DoesNotReconnect()
    {
        var initialConnection = Mock.Of<IConnectionMultiplexer>();
        this.connectionFactoryMock.Setup(x => x.NewConnection()).ReturnsAsync(initialConnection);

        await using var connectionManager = this.CreateRedisConnectionManager();
        await connectionManager.EnsureInitialized();

        this.connectionFactoryMock.Invocations.Clear();
        this.logger.Collector.Clear();

        this.timeProvider.Advance(this.options.MinForcedReconnectionInterval.Add(TimeSpan.FromSeconds(1)));
        await connectionManager.CheckException(new SocketException());

        this.connectionFactoryMock.Verify(x => x.NewConnection(), Times.Never);
        Assert.Same(initialConnection, connectionManager.CurrentConnection);

        LogAssert.SingleEntry(this.logger)
            .HasId(5)
            .HasLevel(LogLevel.Debug)
            .HasMessage("No forced reconnection; first error since last reconnection");
    }

    [Fact]
    public async Task CheckException_FirstErrorSinceReconnection_DoesNotReconnect()
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

        this.connectionFactoryMock.Invocations.Clear();
        this.logger.Collector.Clear();

        this.timeProvider.Advance(this.options.MinForcedReconnectionInterval.Add(TimeSpan.FromSeconds(1)));
        await connectionManager.CheckException(new SocketException());

        this.connectionFactoryMock.Verify(x => x.NewConnection(), Times.Never);
        Assert.Same(secondConnection, connectionManager.CurrentConnection);

        LogAssert.SingleEntry(this.logger)
            .HasId(5)
            .HasLevel(LogLevel.Debug)
            .HasMessage("No forced reconnection; first error since last reconnection");
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

        this.connectionFactoryMock.Invocations.Clear();
        this.logger.Collector.Clear();

        this.timeProvider.Advance(this.options.DroppedConnectionGracePeriod.Subtract(TimeSpan.FromSeconds(1)));
        await connectionManager.CheckException(new SocketException()); // Second error

        this.connectionFactoryMock.Verify(x => x.NewConnection(), Times.Never);
        Assert.Same(initialConnection, connectionManager.CurrentConnection);

        LogAssert.SingleEntry(this.logger)
            .HasId(6)
            .HasLevel(LogLevel.Debug)
            .HasMessage("No forced reconnection; insufficient time has elapsed since first error");
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

        this.connectionFactoryMock.Invocations.Clear();
        this.logger.Collector.Clear();

        this.timeProvider.Advance(this.options.DroppedConnectionGracePeriod.Subtract(TimeSpan.FromSeconds(1)));
        await connectionManager.CheckException(new SocketException()); // Second error since reconnection

        this.connectionFactoryMock.Verify(x => x.NewConnection(), Times.Never);
        Assert.Same(secondConnection, connectionManager.CurrentConnection);

        LogAssert.SingleEntry(this.logger)
            .HasId(6)
            .HasLevel(LogLevel.Debug)
            .HasMessage("No forced reconnection; insufficient time has elapsed since first error");
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

        this.connectionFactoryMock.Invocations.Clear();
        this.logger.Collector.Clear();

        this.timeProvider.Advance(this.options.DroppedConnectionEpisodeTimeout.Add(TimeSpan.FromSeconds(1)));
        await connectionManager.CheckException(new SocketException()); // Second error

        this.connectionFactoryMock.Verify(x => x.NewConnection(), Times.Never);
        Assert.Same(initialConnection, connectionManager.CurrentConnection);

        LogAssert.SingleEntry(this.logger)
            .HasId(7)
            .HasLevel(LogLevel.Debug)
            .HasMessage("No forced reconnection; too much time has elapsed between errors");
    }

    [Fact]
    public async Task CheckException_DroppedConnectionGracePeriodElapsedOnlyIfMeasuredFromFirstError_Reconnects()
    {
        this.connectionFactoryMock
            .Setup(x => x.NewConnection())
            .ReturnsAsync(Mock.Of<IConnectionMultiplexer>());

        await using var connectionManager = this.CreateRedisConnectionManager();
        await connectionManager.EnsureInitialized();

        this.timeProvider.Advance(this.options.MinForcedReconnectionInterval.Add(TimeSpan.FromSeconds(1)));
        await connectionManager.CheckException(new SocketException()); // First error

        this.timeProvider.Advance(this.options.DroppedConnectionGracePeriod.Subtract(TimeSpan.FromSeconds(1)));
        await connectionManager.CheckException(new SocketException()); // Second error

        var newConnection = Mock.Of<IConnectionMultiplexer>();
        this.connectionFactoryMock.Setup(x => x.NewConnection()).ReturnsAsync(newConnection);
        this.logger.Collector.Clear();

        this.timeProvider.Advance(TimeSpan.FromSeconds(2));
        await connectionManager.CheckException(new SocketException()); // Third error

        Assert.Same(newConnection, connectionManager.CurrentConnection);

        new FakeLogRecordAssert(this.logger.Collector.GetSnapshot()[0])
            .HasId(1)
            .HasLevel(LogLevel.Information)
            .HasMessage("Creating new multiplexer");
    }

    [Fact]
    public async Task CheckException_DroppedConnectionEpisodeTimeoutNotElapsedOnlyIfMeasuredFromPreviousError_Reconnects()
    {
        this.connectionFactoryMock
            .Setup(x => x.NewConnection())
            .ReturnsAsync(Mock.Of<IConnectionMultiplexer>());

        await using var connectionManager = this.CreateRedisConnectionManager();
        await connectionManager.EnsureInitialized();

        this.timeProvider.Advance(this.options.MinForcedReconnectionInterval.Add(TimeSpan.FromSeconds(1)));
        await connectionManager.CheckException(new SocketException()); // First error

        this.timeProvider.Advance(this.options.DroppedConnectionGracePeriod.Subtract(TimeSpan.FromSeconds(1)));
        await connectionManager.CheckException(new SocketException()); // Second error

        var newConnection = Mock.Of<IConnectionMultiplexer>();
        this.connectionFactoryMock.Setup(x => x.NewConnection()).ReturnsAsync(newConnection);
        this.logger.Collector.Clear();

        this.timeProvider.Advance(this.options.DroppedConnectionEpisodeTimeout.Subtract(TimeSpan.FromSeconds(1)));
        await connectionManager.CheckException(new SocketException()); // Third error

        Assert.Same(newConnection, connectionManager.CurrentConnection);

        new FakeLogRecordAssert(this.logger.Collector.GetSnapshot()[0])
            .HasId(1)
            .HasLevel(LogLevel.Information)
            .HasMessage("Creating new multiplexer");
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

        new FakeLogRecordAssert(this.logger.Collector.LatestRecord)
            .HasId(2)
            .HasLevel(LogLevel.Information)
            .HasMessage("Disposing old multiplexer");
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

        new FakeLogRecordAssert(this.logger.Collector.LatestRecord)
            .HasId(2)
            .HasLevel(LogLevel.Information)
            .HasMessage("Disposing old multiplexer");
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
