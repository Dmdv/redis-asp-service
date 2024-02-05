using RedisBackgroundService.Services;
using RedisBackgroundService.Tasks;

var builder = WebApplication.CreateBuilder(args);
// This is supposed to run the services concurrently
builder.Services.Configure<HostOptions>(host =>
{
    host.ServicesStartConcurrently = true;
    host.ServicesStopConcurrently = true;
});

builder.Services.AddRouting(o => o.LowercaseUrls = true);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
// Uncomment if you want to use the PublisherService instead of PublisherServiceAsync
// builder.Services.AddHostedService<PublisherService>();
builder.Services.AddHostedService<PublisherServiceAsync>();
builder.Services.AddHostedService<FaultedTask>();
// Uncomment if you want to use the RedisConnector instead of the RedisAsyncConnector
// builder.Services.AddSingleton<RedisConnector>();
builder.Services.AddSingleton<RedisAsyncConnector>();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapControllers();

// Run long running tasks before application starts

app.Run();