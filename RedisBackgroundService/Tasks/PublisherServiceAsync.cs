using RedisBackgroundService.Services;
using StackExchange.Redis;

namespace RedisBackgroundService.Tasks;

// I will not protect _connection assignment for 2 reasons
// 1. PublisherService is a background service. It cannot be started many times
// 2. Lazy is thread-safe by design.

public class PublisherServiceAsync(RedisAsyncConnector redis, ILogger<PublisherService> logger) : BackgroundService
{
    private Task<ConnectionMultiplexer>? _connTask;

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _connTask = redis.ConnectionAsync;
        return base.StartAsync(cancellationToken);
    }

    // Possible alternative implementations may include:
    // 1. Interlocked.CompareExchange(ref _connTask, redis.ConnectionAsync, null);
    // 2. AutoResetEvent
    // 3. SemaphoreSlim
    // 4. ManualResetEventSlim
    // 5. Reactive Observables
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();
        
        var mre = new ManualResetEventSlim(false);
        
        logger.LogInformation("PublisherServiceAsync ExecuteAsync...");

        // This is only to report on the progress of the connection
        var connTask = Task.Run(async () =>
        {
            var connect = await _connTask!;
            logger.LogInformation("PublisherServiceAsync Connection established...");
            mre.Set();
            return connect;
        }, stoppingToken);

        while (!stoppingToken.IsCancellationRequested && 
               !mre.Wait(1000, stoppingToken))
        {
            logger.LogInformation("Waiting for Redis connection: {Time}", DateTimeOffset.Now);
        }

        var connection = await connTask;
        
        if (connection is not { IsConnected: true })
        {
            logger.LogError("Redis connection is null...");
            throw new RedisConnectionException(
                ConnectionFailureType.UnableToConnect,
                "Publisher Service: Redis connection is null...");
        }
        
        connection.ConnectionFailed += (sender, args) =>
        {
            logger.LogError("Connection failed: {Exception}", args.Exception);
        };

        connection.ConnectionRestored += (sender, args) =>
        {
            logger.LogError("Connection restored: {Exception}", args.Exception);
        };

        connection.ErrorMessage += (sender, args) =>
        {
            logger.LogError("Error message: {Message}", args.Message);
        };
        
        logger.LogInformation("Connected to Redis establish...");

        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Publishing message...");

            try
            {
                await connection.GetDatabase(0).PublishAsync(RedisChannel.Literal("updates"), "1");
                logger.LogInformation("Published message at {Time}...", DateTimeOffset.Now);
            }
            catch (RedisConnectionException e)
            {
                logger.LogError("Error while publishing message: {Exception}", e);
            }
            
            await Task.Delay(2000, stoppingToken);
        }
    }
}