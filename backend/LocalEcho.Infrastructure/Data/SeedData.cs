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
        
        // 1. Очистка старых аватарок/загрузок при запуске сидов (по желанию)
        var imgMap = PrepareImages(env);

        // 2. Генерация
        var districts = await SeedDistricts(context, geoFactory);
        var users = await SeedUsers(userManager, roleManager, districts, imgMap);
        await SeedMarkers(context, geoFactory, users, districts, imgMap);
        await SeedReports(context, users);
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
    // Чтобы сиды сработали, в базе не должно быть районов. 
    // Если они есть, но вы хотите добавить новые - закомментируйте 'ifAny' или почистите таблицу Districts.
    if (await context.Districts.AnyAsync()) return await context.Districts.ToListAsync();

    Polygon CreatePoly(double[,] c) {
        var shell = new Coordinate[c.GetLength(0)];
        for (int i = 0; i < c.GetLength(0); i++) shell[i] = new Coordinate(c[i, 1], c[i, 0]);
        return geoFactory.CreatePolygon(shell);
    }

    var list = new List<District> {
        District.Create("Осмоловка", CreatePoly(new[,] {{53.913, 27.556}, {53.916, 27.558}, {53.914, 27.568}, {53.911, 27.566}, {53.913, 27.556}}), "Центр города"),
        District.Create("Маяк Минска", CreatePoly(new[,] {{53.936, 27.648}, {53.938, 27.662}, {53.927, 27.665}, {53.926, 27.648}, {53.936, 27.648}}), "Восток"),
        District.Create("Грушевка", CreatePoly(new[,] {{53.889, 27.514}, {53.894, 27.525}, {53.882, 27.531}, {53.877, 27.521}, {53.889, 27.514}}), "Юго-Запад"),
        District.Create("Курасовщина", CreatePoly(new[,] {{53.855, 27.510}, {53.860, 27.535}, {53.845, 27.535}, {53.840, 27.510}, {53.855, 27.510}}), "Парк Курасовщина"),
        District.Create("Лошица", CreatePoly(new[,] {{53.855, 27.575}, {53.860, 27.595}, {53.845, 27.600}, {53.840, 27.580}, {53.855, 27.575}}), "Жилой массив и усадьба"),
        District.Create("Зеленый Луг", CreatePoly(new[,] {{53.955, 27.620}, {53.955, 27.640}, {53.935, 27.645}, {53.935, 27.625}, {53.955, 27.620}}), "Природа и водоемы"),
        District.Create("Каменная Горка", CreatePoly(new[,] {{53.918, 27.420}, {53.918, 27.460}, {53.905, 27.460}, {53.905, 27.420}, {53.918, 27.420}}), "Минское гетто (активное)"),
        District.Create("Серебрянка", CreatePoly(new[,] {{53.870, 27.595}, {53.875, 27.615}, {53.855, 27.625}, {53.850, 27.605}, {53.870, 27.595}}), "У канала Слепянской системы")
    };

    context.Districts.AddRange(list);
    await context.SaveChangesAsync();
    return list;
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
  private static async Task SeedMarkers(AppDbContext context, GeometryFactory geoFactory, List<ApplicationUser> users, List<District> districts, Dictionary<string, string> imgMap)
    {
        if (await context.Markers.AnyAsync()) return;
        var rand = new Random();
        var imgKeys = imgMap.Keys.Where(k => k.StartsWith("img")).ToList();
        
        var staffAccounts = users.Where(u => u.Email.Contains("echo") || u.Email.Contains("zhes")).ToList();
        var citizens = users.Where(u => !staffAccounts.Contains(u)).ToList();

        for (int i = 0; i < 110; i++)
        {
            var district = districts[rand.Next(districts.Count)];
            var category = (MarkerCategory)rand.Next(0, 5); 
            var author = (category == MarkerCategory.Project) ? staffAccounts[rand.Next(staffAccounts.Count)] : citizens[rand.Next(citizens.Count)];

            // Геометрия
            Geometry location;
            var center = district.Centroid;
            if ((category == MarkerCategory.Project || category == MarkerCategory.Issue) && rand.NextDouble() > 0.8)
            {
                // ПОЛИГОНАЛЬНАЯ МЕТКА
                double s = 0.003;
                location = geoFactory.CreatePolygon(new[] {
                    new Coordinate(center.X - s, center.Y - s), new Coordinate(center.X + s, center.Y - s),
                    new Coordinate(center.X + s, center.Y + s), new Coordinate(center.X - s, center.Y + s),
                    new Coordinate(center.X - s, center.Y - s)
                });
            }
            else
            {
                // ТОЧЕЧНАЯ МЕТКА
                location = geoFactory.CreatePoint(new Coordinate(center.X + (rand.NextDouble() - 0.5) * 0.015, center.Y + (rand.NextDouble() - 0.5) * 0.015));
            }
            location.SRID = 4326;

            var marker = Marker.Create(
                title: GetTitle(category, i), location: location, category: category,
                createdByUserId: author.Id, districtId: district.Id,
                description: "Здесь могло быть ваше подробное описание инцидента. Тестовые данные для анализа просторности интерфейса.",
                scheduledAt: category == MarkerCategory.Event ? DateTime.UtcNow.AddDays(rand.Next(-5, 10)) : null
            );

            marker.UpdateRating(rand.Next(-2, 35));

            // Картинки (ДО)
            int imgCount = rand.Next(0, 4);
            for (int j = 0; j < imgCount; j++)
                marker.Images.Add(MarkerImage.ForMarker(imgMap[imgKeys[rand.Next(imgKeys.Count)]], marker.Id));

            context.Markers.Add(marker);

            // РЕЗОЛЮЦИИ (ХРОНОЛОГИЯ 1:N)
            if ((category == MarkerCategory.Issue || category == MarkerCategory.Suggestion) && rand.NextDouble() > 0.4)
            {
                var staff = staffAccounts[rand.Next(staffAccounts.Count)];
                
                // Первое сообщение: Принято
                var res1 = new MarkerResolution(marker.Id, staff.Id, "Информация принята. Ответственный специалист назначен.");
                context.MarkerResolutions.Add(res1);
                marker.AddResolution(res1);

                // Если шанс выше: Второе сообщение: Результат
                if (rand.NextDouble() > 0.5)
                {
                    var res2 = new MarkerResolution(marker.Id, staff.Id, "Работы выполнены в полном объеме. Посмотрите фото-отчет ниже.");
                    res2.Images.Add(MarkerImage.ForResolution(imgMap["img6"], res2.Id));
                    context.MarkerResolutions.Add(res2);
                    marker.AddResolution(res2);
                    marker.ChangeStatus(MarkerStatus.Resolved);
                }
            }
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
    public static async Task SeedReports(AppDbContext context, List<ApplicationUser> users)
    {
        if (await context.Reports.AnyAsync()) return;
        var rand = new Random();
        var allMarkers = await context.Markers.Take(40).ToListAsync();
        var citizens = users.Where(u => u.Email.Contains("test.by")).ToList();

        foreach (var marker in allMarkers.OrderBy(x => rand.Next()).Take(15))
        {
            int reportCount = rand.Next(1, 4); 
            for (int i = 0; i < reportCount; i++)
            {
                var reporter = citizens[rand.Next(citizens.Count)];
                var reason = (ReportReason)rand.Next(0, 4);
                var report = new Report(marker.Id, reporter.Id, reason, "Текст жалобы: Содержимое метки нарушает правила или является недостоверным.");
                context.Reports.Add(report);
            }
        }
        await context.SaveChangesAsync();
    }
}
