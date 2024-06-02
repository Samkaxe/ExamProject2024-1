using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQService.Controllers;
using System;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register RabbitMQService with HttpClient
builder.Services.AddHttpClient<RabbitMQService.Controllers.RabbitMQService>();
builder.Services.AddSingleton<RabbitMQService.Controllers.RabbitMQService>(sp => new RabbitMQService.Controllers.RabbitMQService("localhost", "checkout_queue", sp.GetRequiredService<HttpClient>()));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();