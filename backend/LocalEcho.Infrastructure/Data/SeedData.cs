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
            var districtsData = new[]
            {
                District.Create("ЖК Северный", 55.7558, 37.6173, "Новый жилой комплекс"),
                District.Create("ТСЖ Центральный", 55.7512, 37.6184, "Старый фонд"),
                District.Create("ЖК Южные высоты", 55.6187, 37.6562, "Высотки"),
                District.Create("Коттеджный посёлок", 55.7245, 37.5543, "Частный сектор"),
                District.Create("ЖК Восточный", 55.7550, 37.7000, "Тестовый"),
                District.Create("ТСЖ Западный", 55.7500, 37.5500, "Тестовый 2")
            };
            context.Districts.AddRange(districtsData);
            await context.SaveChangesAsync();
        }
        
        var districts = await context.Districts.ToListAsync();

        
        async Task CreateUserIfNotExists(string email, string name, string role, Guid districtId)
        {
            if (await userManager.FindByEmailAsync(email) == null)
            {
                var district = districts.FirstOrDefault(d => d.Id == districtId) ?? districts[0];
                var user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    Name = name,
                    DistrictId = district.Id,
                    HomeLatitude = district.CenterLat,
                    HomeLongitude = district.CenterLng,
                    CreatedAt = DateTime.UtcNow,
                    LastSeen = DateTime.UtcNow,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    EmailConfirmed = true,
                    IsVerified = role != "User" 
                };
                
                var result = await userManager.CreateAsync(user, "Pass123!"); 
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, role);
                }
            }
        }

        await CreateUserIfNotExists("user@example.com", "User", "User", districts[0].Id);
        await CreateUserIfNotExists("moderator@example.com", "Moderator", "Moderator", districts[1].Id);
        await CreateUserIfNotExists("admin@example.com", "Admin", "Admin", districts[2].Id);

        if (!await context.Markers.AnyAsync())
        {
            var user1 = await userManager.FindByEmailAsync("user@example.com");
            var user2 = await userManager.FindByEmailAsync("moderator@example.com");
            var user3 = await userManager.FindByEmailAsync("admin@example.com");

            if (user1 != null && user2 != null && user3 != null)
            {
                var markers = new[]
                {
                    Marker.Create("Потоп", new GeoPoint(55.7558, 37.6173), MarkerCategory.Issue, user1.Id, districts[0].Id, "Вода"),
                    Marker.Create("Концерт", new GeoPoint(55.7512, 37.6184), MarkerCategory.Event, user2.Id, districts[1].Id, "Музыка"),
                    Marker.Create("Ремонт", new GeoPoint(55.6187, 37.6562), MarkerCategory.Announcement, user3.Id, districts[2].Id, "Лифт"),
                };

                context.Markers.AddRange(markers);
                await context.SaveChangesAsync();
            }
        }
    }
}