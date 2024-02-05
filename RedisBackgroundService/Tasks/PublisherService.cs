using RedisBackgroundService.Services;
using StackExchange.Redis;

namespace RedisBackgroundService.Tasks;

// I will not protect _connection assignment for 2 reasons
// 1. PublisherService is a background service. It cannot be started many times
// 2. Lazy is thread-safe by design.

public class PublisherService(RedisConnector redis, ILogger<PublisherService> logger) : BackgroundService
{
    private ConnectionMultiplexer? _connection;
    private int _connected;

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("PublisherService StartAsync...");
        
        Task.Run(() =>
        {
            logger.LogInformation("Started initializing Redis connection...");
            
            _connection = redis.Connection;
            Interlocked.Exchange(ref _connected, 1);
            
            logger.LogInformation("Redis connected...");
        }, cancellationToken);
        
        logger.LogInformation("PublisherService StartAsync completed...");
        
        return base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested &&
               Interlocked.CompareExchange(ref _connected, 0, 1) == 0)
        {
            logger.LogInformation("Waiting for Redis connection at: {Time}", DateTimeOffset.Now);
            await Task.Delay(1000, stoppingToken);
        }
        
        if (_connection is not { IsConnected: true })
        {
            logger.LogError("Redis connection is null...");
            throw new RedisConnectionException(
                ConnectionFailureType.UnableToConnect,
                "Publisher Service: Redis connection is null...");
        }
        
        logger.LogInformation("Connected to Redis establish...");

        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Publishing message...");
            
            await _connection.GetDatabase(1).PublishAsync(RedisChannel.Literal("updates"), "1");
            logger.LogInformation("Published message...");
            
            await Task.Delay(1000, stoppingToken);
        }
    }
}