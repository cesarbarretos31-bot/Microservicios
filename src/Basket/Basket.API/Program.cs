using Basket.API.Data;
using Basket.API.Models;
using BuildingBlocks.Behaviors;
using BuildingBlocks.Exceptions.Handler;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Application services
builder.Services.AddCarter();
builder.Services.AddScoped<IBasketRepository, BasketRepository>();
// Si ya tienes CachedBasketRepository, descomenta:
 builder.Services.Decorate<IBasketRepository, CachedBasketRepository>();

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});

// Cross-cutting
builder.Services.AddExceptionHandler<CustomExceptionHandler>();

var app = builder.Build();

builder.Services.AddHealthChecks()
   .AddNpgSql(builder.Configuration.GetConnectionString("Database")!)
   .AddRedis(builder.Configuration.GetConnectionString("Redis")!);

// Pipeline
app.MapCarter();
app.UseExceptionHandler(options => { });

app.UseHealthChecks("/health", new HealthCheckOptions
{

    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.Run();