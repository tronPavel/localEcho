using LocalEcho.Core.Entities;
using LocalEcho.Core.Entities.Identity;
using LocalEcho.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting; 
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
        var env = serviceProvider.GetRequiredService<IWebHostEnvironment>(); // Получаем доступ к файловой системе

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
                District.Create("ЖК Северный", 55.7558, 37.6173, "Новый жилой комплекс", "#FF5733"),
                District.Create("Центральный", 55.7512, 37.6184, "Исторический центр", "#33FF57"),
                District.Create("Южные высоты", 55.6187, 37.6562, "Спальный район", "#3357FF"),
            };
            context.Districts.AddRange(districtsData);
            await context.SaveChangesAsync();
        }
        
        var districts = await context.Districts.ToListAsync();

        async Task<ApplicationUser> CreateUserIfNotExists(string email, string name, string role, int districtIndex)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                var district = districts[districtIndex % districts.Count];
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    Name = name,
                    DistrictId = district.Id,
                    HomeLatitude = district.CenterLat,
                    HomeLongitude = district.CenterLng,
                    CreatedAt = DateTime.UtcNow,
                    LastSeen = DateTime.UtcNow,
                    EmailConfirmed = true,
                    IsVerified = role != "User",
                    AvatarUrl = null 
                };
                
                var result = await userManager.CreateAsync(user, "Pass123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, role);
                }
            }
            return user;
        }

        var user1 = await CreateUserIfNotExists("user@example.com", "Иван Сосед", "User", 0);
        var user2 = await CreateUserIfNotExists("mod@example.com", "Анна Модератор", "Moderator", 1);
        var admin = await CreateUserIfNotExists("admin@example.com", "Админ Админович", "Admin", 2);

        var imageUrls = CopySeedImagesToWwwRoot(env);

        if (!await context.Markers.AnyAsync())
        {
            var markers = new List<Marker>();
            var random = new Random();

            GeoPoint GetRandomLocation(double centerLat, double centerLng)
            {
                var latOffset = (random.NextDouble() - 0.5) * 0.01; 
                var lngOffset = (random.NextDouble() - 0.5) * 0.01;
                return new GeoPoint(centerLat + latOffset, centerLng + lngOffset);
            }

            markers.Add(Marker.Create(
                "Яма на дороге", 
                GetRandomLocation(districts[0].CenterLat, districts[0].CenterLng), 
                MarkerCategory.Issue, 
                user1.Id, 
                districts[0].Id, 
                "Глубокая яма при выезде с парковки, осторожно!", 
                imageUrls.GetValueOrDefault("issue") // Берем картинку
            ));

            markers.Add(Marker.Create(
                "Субботник", 
                GetRandomLocation(districts[0].CenterLat, districts[0].CenterLng), 
                MarkerCategory.Event, 
                user2.Id, 
                districts[0].Id, 
                "Собираемся у первого подъезда, перчатки выдадим.", 
                imageUrls.GetValueOrDefault("event")
            ));

            markers.Add(Marker.Create(
                "Продам гараж", 
                GetRandomLocation(districts[1].CenterLat, districts[1].CenterLng), 
                MarkerCategory.Announcement, 
                admin.Id, 
                districts[1].Id, 
                "Кирпичный, теплый. Звоните.",
                imageUrls.GetValueOrDefault("announcement")
            ));

            for (int i = 0; i < 10; i++)
            {
                var dist = districts[random.Next(districts.Count)];
                markers.Add(Marker.Create(
                    $"Тестовая метка {i}",
                    GetRandomLocation(dist.CenterLat, dist.CenterLng),
                    (MarkerCategory)random.Next(3),
                    user1.Id,
                    dist.Id,
                    "Автоматически сгенерированное описание для проверки кластеризации."
                ));
            }

            context.Markers.AddRange(markers);
            await context.SaveChangesAsync();
        }
    }

    private static Dictionary<string, string> CopySeedImagesToWwwRoot(IWebHostEnvironment env)
    {
        var result = new Dictionary<string, string>();
        
        var seedSourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "SeedImages");
        
        var webRoot = env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var uploadsPath = Path.Combine(webRoot, "uploads");

        if (!Directory.Exists(seedSourcePath)) return result; // Если нет исходников, пропускаем
        if (!Directory.Exists(uploadsPath)) Directory.CreateDirectory(uploadsPath);

        foreach (var filePath in Directory.GetFiles(seedSourcePath))
        {
            var fileName = Path.GetFileName(filePath);
            var destPath = Path.Combine(uploadsPath, fileName);
            
            if (!File.Exists(destPath))
            {
                File.Copy(filePath, destPath);
            }

            var key = Path.GetFileNameWithoutExtension(fileName).ToLower();
            result[key] = $"/uploads/{fileName}";
        }

        return result;
    }
}