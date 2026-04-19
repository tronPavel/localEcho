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
    private static readonly string SimplePass = "Minsk123!";

    public static async Task InitializeAsync(IServiceProvider sp)
    {
        var context = sp.GetRequiredService<AppDbContext>();
        var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = sp.GetRequiredService<RoleManager<ApplicationRole>>();
        var env = sp.GetRequiredService<IWebHostEnvironment>();
        var geoFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

        await context.Database.MigrateAsync();

        var imagesMap = PrepareImages(env);

        var districts = await SeedDistricts(context, geoFactory);
        var users = await SeedUsers(userManager, roleManager, districts, imagesMap);
        await SeedMarkers(context, geoFactory, users, districts, imagesMap);
    }

    private static Dictionary<string, string> PrepareImages(IWebHostEnvironment env)
    {
        var result = new Dictionary<string, string>();
        var sourceDir = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "SeedImages");
        var webRoot = env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

        var avsDir = Path.Combine(webRoot, "avatars");
        var upsDir = Path.Combine(webRoot, "uploads");

        Directory.CreateDirectory(avsDir);
        Directory.CreateDirectory(upsDir);

        if (!Directory.Exists(sourceDir)) return result;

        foreach (var file in Directory.GetFiles(sourceDir))
        {
            var fileName = Path.GetFileName(file);
            var isAvatar = fileName.StartsWith("ava");
            var destDir = isAvatar ? avsDir : upsDir;
            var url = isAvatar ? $"/avatars/{fileName}" : $"/uploads/{fileName}";

            File.Copy(file, Path.Combine(destDir, fileName), true);
            result[Path.GetFileNameWithoutExtension(fileName)] = url;
        }

        return result;
    }
    private static async Task<List<District>> SeedDistricts(AppDbContext context, GeometryFactory geoFactory)
    {
        if (await context.Districts.AnyAsync()) return await context.Districts.ToListAsync();

        Polygon CreateMinskPoly(double[,] coords)
        {
            var shell = new Coordinate[coords.GetLength(0)];
            for (int i = 0; i < coords.GetLength(0); i++)
                shell[i] = new Coordinate(coords[i, 1], coords[i, 0]);
            return geoFactory.CreatePolygon(shell);
        }

        var districts = new List<District>
        {
            District.Create("Осмоловка", CreateMinskPoly(new double[,] {
                {53.913, 27.556}, {53.916, 27.558}, {53.914, 27.568}, {53.911, 27.566}, {53.913, 27.556}
            }), "Исторический центр, культурное наследие"),

            District.Create("Маяк Минска", CreateMinskPoly(new double[,] {
                {53.936, 27.648}, {53.938, 27.662}, {53.927, 27.665}, {53.926, 27.648}, {53.936, 27.648}
            }), "Современный ЖК возле Национальной библиотеки"),

            District.Create("Грушевка", CreateMinskPoly(new double[,] {
                {53.889, 27.514}, {53.894, 27.525}, {53.882, 27.531}, {53.877, 27.521}, {53.889, 27.514}
            }), "Динамично развивающийся высотный квартал")
        };

        context.Districts.AddRange(districts);
        await context.SaveChangesAsync();
        return districts;
    }
    private static async Task<List<ApplicationUser>> SeedUsers(
        UserManager<ApplicationUser> userManager, 
        RoleManager<ApplicationRole> roleManager, 
        List<District> districts,
        Dictionary<string, string> imgMap)
    {
        string[] roles = { "User", "Official", "Moderator", "Admin" };
        foreach (var r in roles)
            if (!await roleManager.RoleExistsAsync(r))
                await roleManager.CreateAsync(new ApplicationRole { Name = r });

        var users = new List<ApplicationUser>();
        var rand = new Random();

        users.Add(await CreateAcc(userManager, "admin@echo.by", "minsk_chief", "Admin", districts[0].Id, 9999, imgMap.GetValueOrDefault("ava1")));
        users.Add(await CreateAcc(userManager, "zhes24@echo.by", "zhes_master", "Official", districts[0].Id, 450, imgMap.GetValueOrDefault("ava2")));
        users.Add(await CreateAcc(userManager, "moderator@echo.by", "city_watch", "Moderator", districts[1].Id, 120, null));

        string[] nicks = { "urban_fox", "ghost_minsk", "green_guy", "techno_daddy", "bicycle_girl", "minsk_sky", "sunny_day", "eco_warrior" };
        foreach (var nick in nicks)
        {
            var ava = rand.NextDouble() > 0.5 ? imgMap.GetValueOrDefault($"ava{rand.Next(1, 4)}") : null;
            users.Add(await CreateAcc(userManager, $"{nick}@test.by", nick, "User", 
                districts[rand.Next(districts.Count)].Id, rand.Next(0, 1500), ava));
        }

        return users;
    }

    private static async Task<ApplicationUser> CreateAcc(UserManager<ApplicationUser> um, string email, string name, string role, Guid dId, int pts, string? ava)
    {
        var user = await um.FindByEmailAsync(email);
        if (user != null) return user;
        user = new ApplicationUser {
            UserName = email, Email = email, Name = name, DistrictId = dId,
            EmailConfirmed = true, Points = pts, AvatarUrl = ava, CreatedAt = DateTime.UtcNow.AddDays(-60)
        };
        await um.CreateAsync(user, SimplePass);
        await um.AddToRoleAsync(user, role);
        return user;
    }
    private static async Task SeedMarkers(
        AppDbContext context, 
        GeometryFactory geoFactory, 
        List<ApplicationUser> users, 
        List<District> districts,
        Dictionary<string, string> imgMap)
    {
        if (await context.Markers.AnyAsync()) return;
        var rand = new Random();
        
        // Список доступных ключей для картинок меток
        var imgKeys = imgMap.Keys.Where(k => k.StartsWith("img")).ToList();
        var officialUser = users.First(u => u.UserName == "zhes24@echo.by");

        for (int i = 0; i < 60; i++)
        {
            var district = districts[rand.Next(districts.Count)];
            var author = users[rand.Next(users.Count)];
            var category = (MarkerCategory)rand.Next(0, 5); // От Issue до Project

            // 1. ГЕНЕРАЦИЯ ГЕОМЕТРИИ (15% шанса на ПОЛИГОН для тестирования Official/Admin)
            Geometry location;
            var center = district.Centroid;
            
            if (rand.Next(1, 100) > 85 && (author.UserName.Contains("admin") || author.UserName.Contains("zhes")))
            {
                // Рисуем квадратную "Зону работ" вокруг центра района
                double offset = 0.002;
                location = geoFactory.CreatePolygon(new[] {
                    new Coordinate(center.X - offset, center.Y - offset),
                    new Coordinate(center.X + offset, center.Y - offset),
                    new Coordinate(center.X + offset, center.Y + offset),
                    new Coordinate(center.X - offset, center.Y + offset),
                    new Coordinate(center.X - offset, center.Y - offset)
                });
            }
            else
            {
                // Обычная точка со случайным смещением от центра района
                double shiftX = (rand.NextDouble() - 0.5) * 0.01;
                double shiftY = (rand.NextDouble() - 0.5) * 0.01;
                location = geoFactory.CreatePoint(new Coordinate(center.X + shiftX, center.Y + shiftY));
            }
            location.SRID = 4326;

            // 2. ДАТЫ ДЛЯ ЭВЕНТОВ (Будущие и прошлые)
            DateTime? schedDate = null;
            if (category == MarkerCategory.Event)
            {
                schedDate = rand.NextDouble() > 0.3 
                    ? DateTime.UtcNow.AddDays(rand.Next(1, 10))  // Будущее (Upcoming)
                    : DateTime.UtcNow.AddDays(rand.Next(-5, -1)); // Прошлое (Passed)
            }

            // 3. СОЗДАНИЕ МАРКЕРА
            var marker = Marker.Create(
                title: GetTitle(category, i),
                location: location,
                category: category,
                createdByUserId: author.Id,
                districtId: rand.NextDouble() > 0.1 ? district.Id : null, // 10% шанс создать "вне района"
                description: "Тестовое описание для проверки интерфейса и отображения текста. Включает детали происшествия и требования по исправлению.",
                scheduledAt: schedDate
            );

            // РЕЙТИНГ (случайный для тестов лидерборда)
            marker.UpdateRating(rand.Next(-5, 45));

            // 4. ДОБАВЛЕНИЕ КАРТИНОК (0, 1 или 3 фото)
            int picRoll = rand.Next(0, 100);
            int count = picRoll < 30 ? 0 : (picRoll < 70 ? 1 : 3);
            
            for (int j = 0; j < count; j++)
            {
                var key = imgKeys[rand.Next(imgKeys.Count)];
                marker.Images.Add(MarkerImage.ForMarker(imgMap[key], marker.Id));
            }

            // 5. РЕЗОЛЮЦИИ (ОФИЦИАЛЬНЫЕ ОТВЕТЫ)
            // Создаем ответы для некоторых Issues и Suggestions
            if ((category == MarkerCategory.Issue || category == MarkerCategory.Suggestion) && rand.NextDouble() > 0.6)
            {
                var resolution = new MarkerResolution(
                    marker.Id, 
                    officialUser.Id, 
                    "Городские службы провели проверку. Проблема устранена / Предложение принято к реализации."
                );

                // В ответ (Resolution) всегда добавляем фото "ПОСЛЕ" (img6)
                if (imgMap.ContainsKey("img6"))
                {
                    resolution.Images.Add(MarkerImage.ForResolution(imgMap["img6"], resolution.Id));
                }

                marker.SetResolution(resolution);
                context.MarkerResolutions.Add(resolution);
            }

            // РУЧНАЯ КОРРЕКТИРОВКА СТАТУСА (для разнообразия)
            if (category == MarkerCategory.Issue && marker.Status != MarkerStatus.Resolved && rand.NextDouble() > 0.5)
            {
                marker.ChangeStatus(MarkerStatus.InProgress);
            }

            context.Markers.Add(marker);
        }

        await context.SaveChangesAsync();
    }

    private static string GetTitle(MarkerCategory cat, int i) => cat switch
    {
        MarkerCategory.Issue => $"Проблема ЖКХ №{i} (Яма/Свет/Мусор)",
        MarkerCategory.Event => $"Мероприятие №{i} (Спорт/Собрание)",
        MarkerCategory.Announcement => $"Объявление №{i} (Поиск/Инфо)",
        MarkerCategory.Suggestion => $"Предложение по району №{i}",
        _ => $"Официальный проект застройки №{i}"
    };
}
