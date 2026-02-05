using LocalEcho.Core.Entities;
using LocalEcho.Infrastructure.Identity;
using LocalEcho.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LocalEcho.Infrastructure.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<AppDbContext>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

        await context.Database.MigrateAsync();

        var roles = new[] { "User", "Moderator", "Admin" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new ApplicationRole { Name = role });
            }
        }

        if (!await context.Districts.AnyAsync())
        {
            var districts = new[]
            {
                District.Create("ЖК Северный", 55.7558, 37.6173, "Новый жилой комплекс на севере города"),
                District.Create("ТСЖ Центральный", 55.7512, 37.6184, "Старый фонд в центре города"),
                District.Create("ЖК Южные высоты", 55.6187, 37.6562, "Высотный комплекс на юге"),
                District.Create("Коттеджный посёлок 'Зеленый'", 55.7245, 37.5543, "Частный сектор на западе"),
                District.Create("ЖК Восточный", 55.7550, 37.7000, "Комплекс на востоке для тестирования"),
                District.Create("ТСЖ Западный", 55.7500, 37.5500, "Фонд на западе для модераторов")
            };
            context.Districts.AddRange(districts);
            await context.SaveChangesAsync();
        }

        if (!await context.Users.AnyAsync())
        {
            var districts = await context.Districts.ToListAsync(); 
            var user1 = new ApplicationUser
            {
                UserName = "user@example.com",
                Email = "user@example.com",
                Name = "user1",
                DistrictId = districts[0].Id, 
                HomeLatitude = districts[0].CenterLat,
                HomeLongitude = districts[0].CenterLng,
                CreatedAt = DateTime.UtcNow,
                LastSeen = DateTime.UtcNow
            };
            await userManager.CreateAsync(user1, "user123");
            await userManager.AddToRoleAsync(user1, "User");

            var moderator = new ApplicationUser
            {
                UserName = "moderator@example.com",
                Email = "moderator@example.com",
                Name = "moderator",
                DistrictId = districts[1].Id,
                HomeLatitude = districts[1].CenterLat,
                HomeLongitude = districts[1].CenterLng,
                IsVerified = true,
                CreatedAt = DateTime.UtcNow,
                LastSeen = DateTime.UtcNow
            };
            await userManager.CreateAsync(moderator, "moderator123");
            await userManager.AddToRoleAsync(moderator, "Moderator");

            var admin = new ApplicationUser
            {
                UserName = "admin@example.com",
                Email = "admin@example.com",
                Name = "admin",
                DistrictId = districts[2].Id, 
                HomeLatitude = districts[2].CenterLat,
                HomeLongitude = districts[2].CenterLng,
                CreatedAt = DateTime.UtcNow,
                LastSeen = DateTime.UtcNow
            };
            await userManager.CreateAsync(admin, "admin123");
            await userManager.AddToRoleAsync(admin, "Admin");
        }

        if (!await context.Markers.AnyAsync())
        {
            var users = await userManager.Users.ToListAsync();
            var districts = await context.Districts.ToListAsync();

            var markers = new[]
            {
                Marker.Create("Потоп на улице", new GeoPoint(55.7558, 37.6173), MarkerCategory.Issue, users[0].Id, districts[0].Id, "Вода по колено"),
                Marker.Create("Концерт в парке", new GeoPoint(55.7512, 37.6184), MarkerCategory.Event, users[1].Id, districts[1].Id, "Бесплатный концерт"),
                Marker.Create("Объявление о ремонте", new GeoPoint(55.6187, 37.6562), MarkerCategory.Announcement, users[2].Id, districts[2].Id, "Ремонт лифта"),
                Marker.Create("Проблема с парковкой", new GeoPoint(55.7245, 37.5543), MarkerCategory.Issue, users[3].Id, districts[3].Id, "Нет мест"),
                Marker.Create("Встреча соседей", new GeoPoint(55.7550, 37.7000), MarkerCategory.Event, users[0].Id, districts[4].Id, "Обсудим проблемы"),
                Marker.Create("Отключение воды", new GeoPoint(55.7500, 37.5500), MarkerCategory.Announcement, users[1].Id, districts[5].Id, "На 2 дня")
            };

            context.Markers.AddRange(markers);
            await context.SaveChangesAsync();
        }
    }
}