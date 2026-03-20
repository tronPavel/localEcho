using LocalEcho.Core.Entities;
using LocalEcho.Core.Entities.Identity;
using LocalEcho.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NetTopologySuite;
using NetTopologySuite.Geometries;

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

        var geoFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

        Polygon CreateSquarePolygon(double centerLat, double centerLng, double offset = 0.02)
        {
            var coordinates = new[]
            {
                new Coordinate(centerLng - offset, centerLat - offset), // Нижний Левый
                new Coordinate(centerLng + offset, centerLat - offset), // Нижний Правый
                new Coordinate(centerLng + offset, centerLat + offset), // Верхний Правый
                new Coordinate(centerLng - offset, centerLat + offset), // Верхний Левый
                new Coordinate(centerLng - offset, centerLat - offset)  // Обязательно замыкаем фигуру
            };
            return geoFactory.CreatePolygon(coordinates);
        }

        var images = CopySeedImagesToWwwRoot(env);

        var roles = new[] { "User", "Moderator", "Admin" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new ApplicationRole { Name = role });
        }

        if (!await context.Districts.AnyAsync())
        {
            context.Districts.AddRange(
                District.Create("ЖК Северный", CreateSquarePolygon(55.7558, 37.6173), "Новый жилой комплекс", "#FF5733"),
                District.Create("Центральный", CreateSquarePolygon(55.7512, 37.6184), "Старый фонд", "#33FF57"),
                District.Create("Парковый", CreateSquarePolygon(55.7245, 37.5543), "Зеленая зона", "#3357FF")
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
                    HomeLocation = district.Centroid,
                    CreatedAt = DateTime.UtcNow,
                    EmailConfirmed = true,
                    IsVerified = role != "User",
                    AvatarUrl = avatarKey != null && images.TryGetValue(avatarKey, out var url) ? url : null,
                    Points = random.Next(10, 100) 
                };

                await userManager.CreateAsync(user, "Pass123!");
                await userManager.AddToRoleAsync(user, role);
            }
            return user;
        }

        users.Add(await CreateUser("user@example.com", "Иван Сосед", "User", "avatar1"));
        users.Add(await CreateUser("mod@example.com", "Анна Модератор", "Moderator", "avatar2"));
        users.Add(await CreateUser("admin@example.com", "Петр Админ", "Admin", "avatar1"));

        for (int i = 1; i <= 15; i++)
        {
            var avatarKey = (i % 3) == 0 ? "avatar1" : ((i % 3) == 1 ? "avatar2" : null);
            users.Add(await CreateUser($"user{i}@test.com", $"Житель {i}", "User", avatarKey));
        }

        var markers = new List<Marker>();

        if (!await context.Markers.AnyAsync())
        {
            var statuses = Enum.GetValues<MarkerStatus>();
            var categories = Enum.GetValues<MarkerCategory>();

            for (int i = 0; i < 30; i++)
            {
                var district = districts[random.Next(districts.Count)];
                var author = users[random.Next(users.Count)];

                var lat = district.Centroid.Y + (random.NextDouble() - 0.5) * 0.05;
                var lng = district.Centroid.X + (random.NextDouble() - 0.5) * 0.05;

                var category = (MarkerCategory)categories.GetValue(random.Next(categories.Length))!;
                var status = (MarkerStatus)statuses.GetValue(random.Next(statuses.Length))!;

                string? imageUrl = category switch
                {
                    MarkerCategory.Issue => images.GetValueOrDefault("issue"),
                    MarkerCategory.Event => images.GetValueOrDefault("event"),
                    MarkerCategory.Announcement => images.GetValueOrDefault("announcement"),
                    _ => null
                };

                var point = geoFactory.CreatePoint(new Coordinate(lng, lat));

                var marker = Marker.Create(
                    $"{category} в {district.Name}",
                    point,
                    category,
                    author.Id,
                    district.Id,
                    $"Это реалистичный маркер #{i + 1}. Ситуация зафиксирована пользователем.",
                    imageUrl
                );

                marker.ChangeStatus(status);
                markers.Add(marker);
            }
            context.Markers.AddRange(markers);
            await context.SaveChangesAsync(); 
        }
        else
        {
            markers = await context.Markers.ToListAsync();
        }

        if (!await context.Votes.AnyAsync() && markers.Any())
        {
            foreach (var marker in markers)
            {
                int totalVotes = random.Next(0, 13);
                if (totalVotes == 0) continue;

                var randomVoters = users.OrderBy(x => random.Next()).Take(totalVotes).ToList();
                var markerCreator = users.FirstOrDefault(u => u.Id == marker.CreatedByUserId);
                
                int ratingDelta = 0;

                foreach (var voter in randomVoters)
                {
                    bool isUpvote = random.NextDouble() > 0.3;
                    var vote = new Vote(marker.Id, voter.Id, isUpvote);
                    
                    context.Votes.Add(vote);
                    
                    int delta = isUpvote ? 1 : -1;
                    ratingDelta += delta;
                }

                marker.UpdateRating(ratingDelta);
                context.Markers.Update(marker);

                if (markerCreator != null)
                {
                    markerCreator.Points += ratingDelta;
                    context.Users.Update(markerCreator); 
                }
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

        if (!Directory.Exists(sourceDir)) return result;

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