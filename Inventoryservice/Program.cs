using System.Text;
using InventoryService.Extintions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using Polly;
using Polly.Extensions.Http;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Define Polly policies with detailed logging
builder.Services.AddLogging(configure => configure.AddConsole());

var loggerFactory = LoggerFactory.Create(logging => logging.AddConsole());
var logger = loggerFactory.CreateLogger("PollyLogger");

var retryPolicy = Policy
    .Handle<MongoException>()
    .WaitAndRetryAsync(3, retryAttempt => 
    {
        var waitTime = TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
        logger.LogWarning($"Retry attempt {retryAttempt} after {waitTime.TotalSeconds} seconds.");
        return waitTime;
    }, onRetry: (exception, timeSpan, retryCount, context) =>
    {
        logger.LogWarning($"Retry {retryCount} executed after {timeSpan.TotalSeconds} seconds due to: {exception.Message}");
    });

var circuitBreakerPolicy = Policy
    .Handle<MongoException>()
    .CircuitBreakerAsync(
        exceptionsAllowedBeforeBreaking: 1,
        durationOfBreak: TimeSpan.FromSeconds(10),
        onBreak: (ex, breakDelay) =>
        {
            logger.LogWarning($"Circuit breaker opened for {breakDelay.TotalMilliseconds}ms due to: {ex.Message}");
        },
        onReset: () => logger.LogInformation("Circuit breaker reset."),
        onHalfOpen: () => logger.LogInformation("Circuit breaker half-open: next call is a trial.")
    );

var policyWrap = Policy.WrapAsync(retryPolicy, circuitBreakerPolicy);

builder.Services.AddSingleton<IAsyncPolicy>(policyWrap);
builder.Services.AddSingleton<MongoService>();

var jwtSecret = builder.Configuration["JwtSettings:Secret"];
var jwtExpirationDays = double.Parse(builder.Configuration["JwtSettings:ExpirationDays"]);
builder.Services.AddSingleton(new JwtTokenService(jwtSecret, jwtExpirationDays));

var key = Encoding.ASCII.GetBytes(jwtSecret);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

builder.Services.AddControllers();

// Swagger Configuration
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
