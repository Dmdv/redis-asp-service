namespace RedisBackgroundService.Tasks;

public class FaultedTask(ILogger<FaultedTask> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Uncomment the following line to simulate a faulted service
        // await Task.Run(() => throw new Exception("FaultedService encountered an error"), stoppingToken);
        await Task.Run(() => logger.LogInformation("Faulted task executed"), stoppingToken);
    }
}