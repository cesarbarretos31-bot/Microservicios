using Basket.API.Data;
using Basket.API.Models;
using BuildingBlocks.Behaviors;
using BuildingBlocks.Exceptions.Handler;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// 1. Application Services & MediatR (Necesario para que el 'sender' funcione en las rutas)
builder.Services.AddCarter();
builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(typeof(Program).Assembly);
    config.AddOpenBehavior(typeof(ValidationBehavior<,>));
    config.AddOpenBehavior(typeof(LoggingBehavior<,>));
});

// 2. Repositories & Caching
builder.Services.AddScoped<IBasketRepository, BasketRepository>();
builder.Services.Decorate<IBasketRepository, CachedBasketRepository>();

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});

// 3. Health Checks (Registrados antes de builder.Build())
builder.Services.AddHealthChecks()
   .AddNpgSql(builder.Configuration.GetConnectionString("Database")!)
   .AddRedis(builder.Configuration.GetConnectionString("Redis")!);

// 4. Cross-cutting (Manejo de excepciones)
builder.Services.AddExceptionHandler<CustomExceptionHandler>();

var app = builder.Build();

// 5. Pipeline HTTP
app.MapCarter();
app.UseExceptionHandler(options => { });

app.UseHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.Run();