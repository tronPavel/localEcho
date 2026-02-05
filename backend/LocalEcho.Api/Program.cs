using LocalEcho.Infrastructure.Data;
using LocalEcho.API.Services; 
using LocalEcho.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using LocalEcho.Application.Interfaces;
using LocalEcho.Application.Services;
using LocalEcho.Core.Interfaces;
using LocalEcho.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Контроллеры + Swagger
builder.Services.AddControllers()
    .AddJsonOptions(opt =>
        opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter())); // enum как строки

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// cors (разрешили все)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllLocal", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
        //.AllowCredentials();            
    });
});
// БД + PostGIS
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsql => npgsql.UseNetTopologySuite() 
    ));

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserContext, UserContext>();

builder.Services.AddScoped<IMarkerRepository, MarkerRepository>();
builder.Services.AddScoped<IDistrictRepository, DistrictRepository>();
builder.Services.AddScoped<IMarkerService, MarkerService>();
builder.Services.AddScoped<IAuthService, AuthService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}
app.UseCors("AllowAllLocal");
app.UseHttpsRedirection();
app.MapControllers();

app.Run();

