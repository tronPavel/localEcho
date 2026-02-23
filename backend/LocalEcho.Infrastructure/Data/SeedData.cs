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
        var env = serviceProvider.GetRequiredService<IWebHostEnvironment>();

        await context.Database.MigrateAsync();

        var images = CopySeedImagesToWwwRoot(env);

        // Создаём роли
        var roles = new[] { "User", "Moderator", "Admin" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new ApplicationRole { Name = role });
        }

        // Создаём районы
        if (!await context.Districts.AnyAsync())
        {
            context.Districts.AddRange(
                District.Create("ЖК Северный", 55.7558, 37.6173, "Новый жилой комплекс", "#FF5733"),
                District.Create("Центральный", 55.7512, 37.6184, "Старый фонд", "#33FF57"),
                District.Create("Парковый", 55.7245, 37.5543, "Зеленая зона", "#3357FF")
            );
            await context.SaveChangesAsync();
        }

        var districts = await context.Districts.ToListAsync();
        var random = new Random();
        var users = new List<ApplicationUser>();

        async Task<ApplicationUser> CreateUser(string email, string name, string role, string? avatarKey)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                var district = districts[random.Next(districts.Count)];

                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    Name = name,
                    DistrictId = district.Id,
                    HomeLatitude = district.CenterLat,
                    HomeLongitude = district.CenterLng,
                    CreatedAt = DateTime.UtcNow,
                    EmailConfirmed = true,
                    IsVerified = role != "User",
                    AvatarUrl = avatarKey != null && images.TryGetValue(avatarKey, out var url) ? url : null,
                    Points = random.Next(50, 650)
                };

                await userManager.CreateAsync(user, "Pass123!");
                await userManager.AddToRoleAsync(user, role);
            }
            return user;
        }

        // Основные пользователи с аватарками
        users.Add(await CreateUser("user@example.com", "Иван Сосед", "User", "avatar1"));
        users.Add(await CreateUser("mod@example.com", "Анна Модератор", "Moderator", "avatar2"));
        users.Add(await CreateUser("admin@example.com", "Петр Админ", "Admin", "avatar1"));

        // Боты с аватарками
        for (int i = 1; i <= 15; i++)
        {
            var avatarKey = (i % 3) switch
            {
                0 => "avatar1",
                1 => "avatar2",
                _ => null
            };

            users.Add(await CreateUser($"user{i}@test.com", $"Житель {i}", "User", avatarKey));
        }

        // Создаём маркеры с картинками
        if (!await context.Markers.AnyAsync())
        {
            var statuses = Enum.GetValues<MarkerStatus>();
            var categories = Enum.GetValues<MarkerCategory>();

            for (int i = 0; i < 30; i++)
            {
                var district = districts[random.Next(districts.Count)];
                var author = users[random.Next(users.Count)];

                var lat = district.CenterLat + (random.NextDouble() - 0.5) * 0.05;
                var lng = district.CenterLng + (random.NextDouble() - 0.5) * 0.05;

                var category = (MarkerCategory)categories.GetValue(random.Next(categories.Length))!;

                string? imageUrl = category switch
                {
                    MarkerCategory.Issue => images.GetValueOrDefault("issue"),
                    MarkerCategory.Event => images.GetValueOrDefault("event"),
                    MarkerCategory.Announcement => images.GetValueOrDefault("announcement"),
                    _ => null
                };

                var marker = Marker.Create(
                    $"{category} в {district.Name}",
                    new GeoPoint(lat, lng),
                    category,
                    author.Id,
                    district.Id,
                    $"Это тестовый маркер #{i + 1}. Реальная ситуация в районе.",
                    imageUrl
                );

                marker.ChangeStatus((MarkerStatus)statuses.GetValue(random.Next(statuses.Length))!);

                context.Markers.Add(marker);
            }

            await context.SaveChangesAsync();
        }
    }

    private static Dictionary<string, string> CopySeedImagesToWwwRoot(IWebHostEnvironment env)
    {
        var result = new Dictionary<string, string>();

        var sourceDir = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "SeedImages");
        var webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

        var avatarsDir = Path.Combine(webRoot, "avatars");
        var uploadsDir = Path.Combine(webRoot, "uploads");

        Directory.CreateDirectory(avatarsDir);
        Directory.CreateDirectory(uploadsDir);

        if (!Directory.Exists(sourceDir))
            return result;

        foreach (var file in Directory.GetFiles(sourceDir))
        {
            var fileName = Path.GetFileName(file);
            var key = Path.GetFileNameWithoutExtension(file).ToLowerInvariant();

            var isAvatar = key.Contains("avatar");
            var destDir = isAvatar ? avatarsDir : uploadsDir;
            var url = isAvatar ? $"/avatars/{fileName}" : $"/uploads/{fileName}";

            var destPath = Path.Combine(destDir, fileName);
            File.Copy(file, destPath, true);

            result[key] = url;
        }

        return result;
    }
}