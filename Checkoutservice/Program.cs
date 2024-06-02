using Checkoutservice.Extintions;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Polly;
using Serilog;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Load the configuration from appsettings.json
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

// Register the configuration class
builder.Services.Configure<FeatureToggleSettings>(builder.Configuration.GetSection("FeatureToggles"));

// Add services to the container
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

// Register RedisCacheService with dependency injection
builder.Services.AddSingleton<RedisCacheService>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var logger = provider.GetRequiredService<ILogger<RedisCacheService>>();
    var connectionString = configuration["RedisConnection:ConnectionString"];
    var featureToggleSettings = provider.GetRequiredService<IOptions<FeatureToggleSettings>>().Value;
    return new RedisCacheService(connectionString, logger, retryPolicy, circuitBreakerPolicy, featureToggleSettings);
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
            .AddMongoDBInstrumentation()
            .AddZipkinExporter(options =>
            {
                options.Endpoint = new Uri("http://zipkin:9411/api/v2/spans");
            });
    });

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
