namespace RedisBackgroundService.Services;

using System;
using StackExchange.Redis;

public class RedisConnector
{
    private readonly Lazy<ConnectionMultiplexer> _lazyConnection;

    public RedisConnector(ILogger<RedisConnector> logger)
    {
        logger.LogInformation("RedisService: Initializing...");

        var options = new ConfigurationOptions
        {
            ClientName = "RedisCachingServer",
            SyncTimeout = 10,
            AbortOnConnectFail = false,
            ConnectRetry = 3,
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

        _lazyConnection = new Lazy<ConnectionMultiplexer>(
            () =>
            {
                var connect = ConnectionMultiplexer.Connect(options);
                connect.ConnectionFailed += (sender, args) =>
                {
                    logger.LogError("Connection failed: {Exception}", args.Exception);
                };

                connect.ConnectionRestored += (sender, args) =>
                {
                    logger.LogError("Connection restored: {Exception}", args.Exception);
                };

                connect.ErrorMessage += (sender, args) =>
                {
                    logger.LogError("Error message: {Message}", args.Message);
                };

                return connect;
            }, LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public ConnectionMultiplexer Connection => _lazyConnection.Value;
}