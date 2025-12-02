using LocalEcho.Infrastructure.Data;
using LocalEcho.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Контроллеры + Swagger
builder.Services.AddControllers()
    .AddJsonOptions(opt =>
        opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter())); // enum как строки

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// БД + PostGIS
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsql => npgsql.UseNetTopologySuite() // ← обязательно!
    ));

// DI
builder.Services.AddScoped<IMarkerRepository, MarkerRepository>();
builder.Services.AddScoped<IMarkerService, MarkerService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Автоматически применяем миграции при старте (удобно на разработке)
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();