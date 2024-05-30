using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Polly;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.Seq(builder.Configuration["Seq:ServerUrl"])
    .CreateLogger();

builder.Host.UseSerilog();

// Configure Polly policies
var retryPolicy = Policy.Handle<Exception>()
    .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
        onRetry: (exception, timeSpan, retryCount, context) =>
        {
            Log.Warning("Retry {RetryCount} due to {Exception}.", retryCount, exception);
        });

var circuitBreakerPolicy = Policy.Handle<Exception>()
    .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1),
        onBreak: (exception, duration) =>
        {
            Log.Warning("Circuit broken for {Duration} due to {Exception}.", duration, exception);
        },
        onReset: () => Log.Information("Circuit reset."),
        onHalfOpen: () => Log.Information("Circuit in half-open state."));

builder.Services.AddSingleton<RedisCacheService>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var logger = provider.GetRequiredService<ILogger<RedisCacheService>>();
    var connectionString = configuration["RedisConnection:ConnectionString"];
    return new RedisCacheService(connectionString, logger, retryPolicy, circuitBreakerPolicy);
});

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Configure OpenTelemetry with Zipkin
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resourceBuilder => resourceBuilder.AddService("CheckoutService"))
    .WithTracing(tracingBuilder =>
    {
        tracingBuilder
            .AddSource("CheckoutService")
            .SetSampler(new AlwaysOnSampler())
            .AddAspNetCoreInstrumentation()
            //.AddConsoleExporter()
            .AddZipkinExporter(options =>
            {
                options.Endpoint = new Uri("http://zipkin:9411/api/v2/spans");
            });
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
