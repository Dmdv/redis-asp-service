namespace RedisBackgroundService.Services;

using System;
using StackExchange.Redis;

public class RedisAsyncConnector
{
    private readonly Lazy<Task<ConnectionMultiplexer>> _lazyConnection;

    public RedisAsyncConnector(ILogger<RedisAsyncConnector> logger)
    {
        logger.LogInformation("RedisService: Initializing...");

        var options = new ConfigurationOptions
        {
            ClientName = "RedisCachingServer",
            SyncTimeout = 1000,
            ConnectTimeout = 5000,
            AbortOnConnectFail = false,
            ConnectRetry = 25,
            EndPoints =
            {
                {
                    "127.0.0.1",
                    6379
                }
            },
            DefaultDatabase = 0,
            ReconnectRetryPolicy = new ExponentialRetry(1000),
            Password = "",
            AllowAdmin = true
        };

        _lazyConnection = new Lazy<Task<ConnectionMultiplexer>>(
            () =>
            {
                var connect = ConnectionMultiplexer.ConnectAsync(options);
                return connect;
            }, LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public Task<ConnectionMultiplexer> ConnectionAsync => _lazyConnection.Value;
}