using System.Net;
using Microsoft.Extensions.Logging;
using Moq;
using OneClickYatra.Api.Services;
using StackExchange.Redis;

namespace OneClickYatra.UnitTests.Services;

public class RedisCacheServiceTests
{
    private sealed class CachedValue
    {
        public string Name { get; set; } = string.Empty;
    }

    private readonly Mock<IConnectionMultiplexer> _connectionMultiplexer = new();
    private readonly Mock<IDatabase> _database = new();

    private RedisCacheService CreateSut(bool __isConnected = true)
    {
        _connectionMultiplexer.Setup(c => c.IsConnected).Returns(__isConnected);
        _connectionMultiplexer
            .Setup(c => c.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
            .Returns(_database.Object);
        return new RedisCacheService(_connectionMultiplexer.Object, Mock.Of<ILogger<RedisCacheService>>());
    }

    /// <summary>Wires the mocked IDatabase's string GET/SET/DELETE calls to an in-memory
    /// dictionary, so tests can exercise the real cache-aside round trip through
    /// RedisCacheService without a real Redis server. Every mocked overload below spells out
    /// every parameter of the exact StackExchange.Redis overload RedisCacheService.cs resolves to
    /// (Moq's expression-tree Setup cannot rely on C# filling in optional arguments).</summary>
    private void WireStringStore(Dictionary<RedisKey, RedisValue> __store)
    {
        _database
            .Setup(d => d.StringSetAsync(
                It.IsAny<RedisKey>(),
                It.IsAny<RedisValue>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<bool>(),
                It.IsAny<When>(),
                It.IsAny<CommandFlags>()))
            .Callback<RedisKey, RedisValue, TimeSpan?, bool, When, CommandFlags>((key, value, _, _, _, _) => __store[key] = value)
            .ReturnsAsync(true);

        _database
            .Setup(d => d.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .Returns<RedisKey, CommandFlags>((key, _) => Task.FromResult(__store.TryGetValue(key, out var value) ? value : RedisValue.Null));

        _database
            .Setup(d => d.KeyDeleteAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .Returns<RedisKey, CommandFlags>((key, _) => Task.FromResult(__store.Remove(key)));
    }

    private static async IAsyncEnumerable<RedisKey> ToAsyncEnumerable(IEnumerable<RedisKey> __keys)
    {
        foreach (var key in __keys)
        {
            yield return key;
        }
        await Task.CompletedTask;
    }

    [Fact]
    public async Task GetAsync_AfterSet_ReturnsThePreviouslySetValue()
    {
        var sut = CreateSut();
        WireStringStore([]);

        await sut.SetAsync("package:id:1", new CachedValue { Name = "Goa Family Getaway" }, TimeSpan.FromMinutes(10), CancellationToken.None);
        var result = await sut.GetAsync<CachedValue>("package:id:1", CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Goa Family Getaway", result!.Name);
    }

    [Fact]
    public async Task GetAsync_KeyWasNeverSet_ReturnsNull()
    {
        var sut = CreateSut();
        WireStringStore([]);

        var result = await sut.GetAsync<CachedValue>("package:id:does-not-exist", CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAsync_AfterRemove_ReturnsNullAgain()
    {
        var sut = CreateSut();
        WireStringStore([]);

        await sut.SetAsync("package:id:1", new CachedValue { Name = "Goa" }, TimeSpan.FromMinutes(10), CancellationToken.None);
        Assert.NotNull(await sut.GetAsync<CachedValue>("package:id:1", CancellationToken.None));

        await sut.RemoveAsync("package:id:1", CancellationToken.None);
        var result = await sut.GetAsync<CachedValue>("package:id:1", CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAsync_AfterRemoveByPrefix_ReturnsNullAgain()
    {
        var sut = CreateSut();
        var store = new Dictionary<RedisKey, RedisValue>();
        WireStringStore(store);

        await sut.SetAsync("package:id:1", new CachedValue { Name = "Goa" }, TimeSpan.FromMinutes(10), CancellationToken.None);
        await sut.SetAsync("package:slug:goa-family-getaway", new CachedValue { Name = "Goa" }, TimeSpan.FromMinutes(10), CancellationToken.None);
        Assert.NotNull(await sut.GetAsync<CachedValue>("package:id:1", CancellationToken.None));

        var server = new Mock<IServer>();
        server
            .Setup(s => s.KeysAsync(
                It.IsAny<int>(),
                It.IsAny<RedisValue>(),
                It.IsAny<int>(),
                It.IsAny<long>(),
                It.IsAny<int>(),
                It.IsAny<CommandFlags>()))
            .Returns(ToAsyncEnumerable(store.Keys.ToList()));

        EndPoint endpoint = new IPEndPoint(IPAddress.Loopback, 6379);
        _connectionMultiplexer.Setup(c => c.GetEndPoints(It.IsAny<bool>())).Returns([endpoint]);
        _connectionMultiplexer
            .Setup(c => c.GetServer(It.Is<EndPoint>(e => e == endpoint), It.IsAny<object>()))
            .Returns(server.Object);

        await sut.RemoveByPrefixAsync("package:", CancellationToken.None);

        Assert.Null(await sut.GetAsync<CachedValue>("package:id:1", CancellationToken.None));
        Assert.Null(await sut.GetAsync<CachedValue>("package:slug:goa-family-getaway", CancellationToken.None));
    }

    [Fact]
    public async Task GetAsync_RedisNotConnected_DegradesToCacheMissWithoutTouchingTheDatabase()
    {
        var sut = CreateSut(__isConnected: false);

        var result = await sut.GetAsync<CachedValue>("package:id:1", CancellationToken.None);

        Assert.Null(result);
        _connectionMultiplexer.Verify(c => c.GetDatabase(It.IsAny<int>(), It.IsAny<object>()), Times.Never);
    }

    [Fact]
    public async Task SetAsync_RedisNotConnected_NoOpsWithoutThrowing()
    {
        var sut = CreateSut(__isConnected: false);

        await sut.SetAsync("package:id:1", new CachedValue { Name = "Goa" }, TimeSpan.FromMinutes(10), CancellationToken.None);

        _connectionMultiplexer.Verify(c => c.GetDatabase(It.IsAny<int>(), It.IsAny<object>()), Times.Never);
    }

    [Fact]
    public async Task RemoveAsync_RedisNotConnected_NoOpsWithoutThrowing()
    {
        var sut = CreateSut(__isConnected: false);

        await sut.RemoveAsync("package:id:1", CancellationToken.None);

        _connectionMultiplexer.Verify(c => c.GetDatabase(It.IsAny<int>(), It.IsAny<object>()), Times.Never);
    }

    [Fact]
    public async Task RemoveByPrefixAsync_RedisNotConnected_NoOpsWithoutThrowing()
    {
        var sut = CreateSut(__isConnected: false);

        await sut.RemoveByPrefixAsync("package:", CancellationToken.None);

        _connectionMultiplexer.Verify(c => c.GetEndPoints(It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task GetAsync_RedisThrowsMidCall_DegradesToCacheMissInsteadOfThrowing()
    {
        var sut = CreateSut();
        _database
            .Setup(d => d.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ThrowsAsync(new RedisConnectionException(ConnectionFailureType.SocketFailure, "Connection lost"));

        var result = await sut.GetAsync<CachedValue>("package:id:1", CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task SetAsync_RedisThrowsMidCall_DoesNotPropagateTheException()
    {
        var sut = CreateSut();
        _database
            .Setup(d => d.StringSetAsync(
                It.IsAny<RedisKey>(),
                It.IsAny<RedisValue>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<bool>(),
                It.IsAny<When>(),
                It.IsAny<CommandFlags>()))
            .ThrowsAsync(new RedisConnectionException(ConnectionFailureType.SocketFailure, "Connection lost"));

        await sut.SetAsync("package:id:1", new CachedValue { Name = "Goa" }, TimeSpan.FromMinutes(10), CancellationToken.None);
    }
}
