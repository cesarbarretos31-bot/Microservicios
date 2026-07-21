using Basket.API.Data;
using Basket.API.Models;
using BuildingBlocks.Behaviors;
using BuildingBlocks.Exceptions.Handler;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Marten;
using Carter;

var builder = WebApplication.CreateBuilder(args);

// 1. Configuración de CORS (Servicio necesario para que el frontend funcione)
builder.Services.AddCors();

// 2. Carter y MediatR (Resuelve el error del "sender" en las rutas)
builder.Services.AddCarter();
builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(typeof(Program).Assembly);
    config.AddOpenBehavior(typeof(ValidationBehavior<,>));
    config.AddOpenBehavior(typeof(LoggingBehavior<,>));
});

// 3. Marten / PostgreSQL (Resuelve el error de IDocumentSession)
builder.Services.AddMarten(opts =>
{
    opts.Connection(builder.Configuration.GetConnectionString("Database")!);
    opts.Schema.For<ShoppingCart>().Identity(x => x.UserName);
}).UseLightweightSessions();

// 4. Repositorios
builder.Services.AddScoped<IBasketRepository, BasketRepository>();
builder.Services.Decorate<IBasketRepository, CachedBasketRepository>();

// 5. Redis Caché
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});

// 6. Manejo de Excepciones y HealthChecks (Acomodados ANTES del Build)
builder.Services.AddExceptionHandler<CustomExceptionHandler>();

builder.Services.AddHealthChecks()
   .AddNpgSql(builder.Configuration.GetConnectionString("Database")!)
   .AddRedis(builder.Configuration.GetConnectionString("Redis")!);

// ==========================================
// CONSTRUCCIÓN DE LA APLICACIÓN
// ==========================================
var app = builder.Build();

// 7. Configuración del Pipeline HTTP
app.UseCors(policy =>
    policy.AllowAnyOrigin()
          .AllowAnyMethod()
          .AllowAnyHeader());

app.MapCarter();
app.UseExceptionHandler(options => { });

app.UseHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.Run();