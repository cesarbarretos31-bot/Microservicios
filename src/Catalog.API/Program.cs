var builder = WebApplication.CreateBuilder(args);

// 1. Agregar el servicio de CORS (¡Esto te faltaba!)
builder.Services.AddCors();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Services.AddCarter();

builder.Services.AddMarten(opts =>
{
    opts.Connection(builder.Configuration.GetConnectionString("Database")!);
}).UseLightweightSessions();

var app = builder.Build();

// 2. Aplicar la política de CORS (Antes de MapCarter)
app.UseCors(policy =>
    policy.AllowAnyOrigin()
          .AllowAnyMethod()
          .AllowAnyHeader());

app.MapCarter();

app.Run();