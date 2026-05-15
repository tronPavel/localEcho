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
    private static readonly string CommonPass = "Minsk123!";
    private static readonly Random Rnd = new();
    private static readonly GeometryFactory _geoFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

    public static async Task InitializeAsync(IServiceProvider sp)
    {
        var context = sp.GetRequiredService<AppDbContext>();
        var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = sp.GetRequiredService<RoleManager<ApplicationRole>>();
        var env = sp.GetRequiredService<IWebHostEnvironment>();

        await ClearDatabase(context);

        string[] roles = { "User", "Official", "Moderator", "Admin" };
        foreach (var r in roles)
            if (!await roleManager.RoleExistsAsync(r))
                await roleManager.CreateAsync(new ApplicationRole { Name = r });

        var imgMap = PrepareImages(env);

        var cities = await SeedCities(context);

        var districts = await SeedDistricts(context, cities);

        var users = await SeedUsers(userManager, cities, districts, imgMap);

        await SeedMarkers(context, users, cities, districts, imgMap);

        await SeedActivity(context, users);
    }

    private static async Task ClearDatabase(AppDbContext context)
    {
        var tables = new[] 
        { 
            "Reports", "Votes", "MarkerImages", "MarkerResolutions", 
            "Markers", "Districts", "Cities", "AspNetUserRoles", "AspNetUsers" 
        };

        foreach (var table in tables)
        {
            await context.Database.ExecuteSqlRawAsync($"TRUNCATE TABLE \"{table}\" RESTART IDENTITY CASCADE;");
        }
    }

    private static Polygon CreatePolygon(double[][] coords)
    {
        var points = coords.Select(c => new Coordinate(c[1], c[0])).ToArray();
        
        if (!points[0].Equals2D(points[^1]))
        {
            var closedPoints = new Coordinate[points.Length + 1];
            points.CopyTo(closedPoints, 0);
            closedPoints[^1] = points[0];
            points = closedPoints;
        }

        var poly = _geoFactory.CreatePolygon(points);
        poly.SRID = 4326;
        return poly;
    }
    private static async Task<List<City>> SeedCities(AppDbContext context)
    {
        var cities = new List<City>
        {
            City.Create("Минск", CreatePolygon(new[] {
                new[] {53.968, 27.402}, new[] {53.968, 27.684}, new[] {53.826, 27.697}, 
                new[] {53.820, 27.408}, new[] {53.968, 27.402}
            })),
            City.Create("Брест", CreatePolygon(new[] {
                new[] {52.148, 23.570}, new[] {52.155, 23.820}, new[] {52.045, 23.820}, 
                new[] {52.045, 23.600}, new[] {52.148, 23.570}
            })),
            City.Create("Гродно", CreatePolygon(new[] {
                new[] {53.725, 23.730}, new[] {53.735, 23.940}, new[] {53.595, 23.940}, 
                new[] {53.605, 23.715}, new[] {53.725, 23.730}
            }))
        };
        context.Cities.AddRange(cities);
        await context.SaveChangesAsync();
        return cities;
    }

  private static async Task<List<District>> SeedDistricts(AppDbContext context, List<City> cities)
{
    var districts = new List<District>();

    var minskId = cities.First(c => c.Name == "Минск").Id;
    var mData = new List<(string Name, double[][] Coords)>
    {
        ("Центральный", new[] { new[] {53.901, 27.530}, new[] {53.951, 27.480}, new[] {53.961, 27.551}, new[] {53.921, 27.575}, new[] {53.901, 27.561} }),
        ("Советский", new[] { new[] {53.916, 27.576}, new[] {53.961, 27.586}, new[] {53.963, 27.630}, new[] {53.951, 27.641}, new[] {53.921, 27.601} }),
        ("Первомайский", new[] { new[] {53.921, 27.611}, new[] {53.976, 27.691}, new[] {53.945, 27.760}, new[] {53.921, 27.751}, new[] {53.906, 27.651} }),
        ("Партизанский", new[] { new[] {53.881, 27.611}, new[] {53.916, 27.621}, new[] {53.920, 27.750}, new[] {53.861, 27.751}, new[] {53.860, 27.660} }),
        ("Заводской", new[] { new[] {53.821, 27.631}, new[] {53.881, 27.631}, new[] {53.900, 27.700}, new[] {53.891, 27.740}, new[] {53.831, 27.721} }),
        ("Ленинский", new[] { new[] {53.831, 27.571}, new[] {53.880, 27.550}, new[] {53.896, 27.561}, new[] {53.901, 27.606}, new[] {53.841, 27.626} }),
        ("Октябрьский", new[] { new[] {53.811, 27.511}, new[] {53.840, 27.500}, new[] {53.886, 27.516}, new[] {53.886, 27.556}, new[] {53.831, 27.566} }),
        ("Московский", new[] { new[] {53.831, 27.416}, new[] {53.860, 27.430}, new[] {53.896, 27.471}, new[] {53.901, 27.541}, new[] {53.861, 27.531} }),
        ("Фрунзенский", new[] { new[] {53.896, 27.401}, new[] {53.946, 27.411}, new[] {53.960, 27.450}, new[] {53.936, 27.526}, new[] {53.901, 27.516} })
    };

    var grodnoId = cities.First(c => c.Name == "Гродно").Id;
    var gData = new List<(string Name, double[][] Coords)>
    {
        ("Центр (Старый город)", new[] { 
            new[] {53.677, 23.820}, new[] {53.682, 23.810}, new[] {53.689, 23.812}, 
            new[] {53.693, 23.825}, new[] {53.690, 23.845}, new[] {53.685, 23.852}, 
            new[] {53.676, 23.842}, new[] {53.674, 23.828} 
        }),
        ("Девятовка", new[] { 
            new[] {53.694, 23.845}, new[] {53.710, 23.830}, new[] {53.725, 23.845}, 
            new[] {53.730, 23.865}, new[] {53.715, 23.880}, new[] {53.698, 23.875}, 
            new[] {53.694, 23.860}
        }),
        ("Ольшанка", new[] { 
            new[] {53.596, 23.810}, new[] {53.605, 23.795}, new[] {53.620, 23.805}, 
            new[] {53.628, 23.820}, new[] {53.620, 23.845}, new[] {53.608, 23.848}, 
            new[] {53.598, 23.835}
        }),
        ("Вишневец", new[] { 
            new[] {53.635, 23.848}, new[] {53.655, 23.840}, new[] {53.665, 23.860}, 
            new[] {53.655, 23.885}, new[] {53.638, 23.888}, new[] {53.625, 23.870}
        }),
        ("Форты", new[] { 
            new[] {53.694, 23.785}, new[] {53.715, 23.775}, new[] {53.725, 23.795}, 
            new[] {53.718, 23.820}, new[] {53.700, 23.825}, new[] {53.685, 23.810}
        }),
        ("Фолюш", new[] { 
            new[] {53.658, 23.780}, new[] {53.668, 23.765}, new[] {53.682, 23.780}, 
            new[] {53.678, 23.810}, new[] {53.660, 23.812}, new[] {53.652, 23.795}
        })
    };

    var brestId = cities.First(c => c.Name == "Брест").Id;
    var bData = new List<(string Name, double[][] Coords)>
    {
        ("Исторический центр", new[] { 
            new[] {52.085, 23.682}, new[] {52.090, 23.668}, new[] {52.105, 23.670}, 
            new[] {52.112, 23.690}, new[] {52.100, 23.715}, new[] {52.088, 23.708}
        }),
        ("Брестская Крепость", new[] { 
            new[] {52.080, 23.650}, new[] {52.088, 23.645}, new[] {52.094, 23.655}, 
            new[] {52.090, 23.674}, new[] {52.082, 23.670}, new[] {52.078, 23.660}
        }),
        ("Ковалево", new[] { 
            new[] {52.064, 23.738}, new[] {52.082, 23.732}, new[] {52.090, 23.750}, 
            new[] {52.085, 23.778}, new[] {52.070, 23.785}, new[] {52.058, 23.765},
            new[] {52.060, 23.745}
        }),
        ("Восток", new[] { 
            new[] {52.095, 23.742}, new[] {52.115, 23.735}, new[] {52.128, 23.755}, 
            new[] {52.122, 23.795}, new[] {52.100, 23.805}, new[] {52.092, 23.775}
        }),
        ("Вулька", new[] { 
            new[] {52.055, 23.695}, new[] {52.078, 23.690}, new[] {52.084, 23.720}, 
            new[] {52.072, 23.735}, new[] {52.052, 23.730}, new[] {52.048, 23.715}
        }),
        ("Речица (Брест)", new[] { 
            new[] {52.110, 23.625}, new[] {52.128, 23.615}, new[] {52.145, 23.630}, 
            new[] {52.138, 23.670}, new[] {52.112, 23.678}, new[] {52.105, 23.655}
        })
    };

    AddDistricts(districts, mData, minskId);
    AddDistricts(districts, gData, grodnoId);
    AddDistricts(districts, bData, brestId);

    context.Districts.AddRange(districts);
    await context.SaveChangesAsync();
    return districts;
}

private static void AddDistricts(List<District> list, List<(string Name, double[][] Coords)> data, Guid cityId)
{
    foreach (var d in data)
    {
        list.Add(District.Create(d.Name, CreatePolygon(d.Coords), cityId, $"Отработанная геометрия для {d.Name}"));
    }
}
private static async Task SeedMarkers(AppDbContext context, List<ApplicationUser> users, List<City> cities, List<District> districts, Dictionary<string, string> imgMap)
{
    var imgKeys = imgMap.Keys.Where(k => k.StartsWith("img") && k != "img_result").ToList();
    var residents = users.Where(u => u.Email.Contains("@test.by")).ToList();
    var officials = users.Where(u => u.Email.Contains("@echo.by")).ToList();

    string[] titles = { "Выбоина на дороге", "Не работает фонарь", "Стихийная свалка", "Поломка на детской площадке", "Нужно озеленение", "Городской пикник", "Проект велодорожки", "Разбита плитка" };
    
    for (int i = 0; i < 160; i++)
    {
        var district = districts[Rnd.Next(districts.Count)];
        var category = (MarkerCategory)Rnd.Next(0, 5);
        
        var isOfficial = (category == MarkerCategory.Project) || (category == MarkerCategory.Event) || (Rnd.NextDouble() > 0.9);
        var author = isOfficial ? officials[Rnd.Next(officials.Count)] : residents[Rnd.Next(residents.Count)];
        
        var location = GetRandomPointInPolygon(district.Boundaries);
        Geometry finalGeom = location;
        
        if (Rnd.NextDouble() > 0.8)
        {
            double s = 0.0012;
            finalGeom = _geoFactory.CreatePolygon(new Coordinate[] {
                new(location.X, location.Y), new(location.X + s, location.Y),
                new(location.X + s, location.Y + s), new(location.X, location.Y + s),
                new(location.X, location.Y)
            });
        }
        finalGeom.SRID = 4326;

        DateTime? start = null; DateTime? end = null;
        if (category == MarkerCategory.Event) { start = DateTime.UtcNow.AddDays(Rnd.Next(1, 10)); end = start.Value.AddDays(2); }

        var marker = Marker.Create(
            $"{titles[Rnd.Next(titles.Length)]} #{i}",
            finalGeom, category, author.Id, district.Id, district.CityId, isOfficial,
            "Описание составлено жителем района. Просим обратить внимание на текущее состояние объекта и принять меры.",
            start, end
        );

        marker.UpdateRating(Rnd.Next(-5, 80));

        int photoCount = Rnd.Next(1, 4); 
        for (int j = 0; j < photoCount; j++)
            marker.Images.Add(MarkerImage.ForMarker(imgMap[imgKeys[Rnd.Next(imgKeys.Count)]], marker.Id));

        if ((category == MarkerCategory.Issue || category == MarkerCategory.Suggestion) && Rnd.NextDouble() > 0.4)
        {
            var cityOfficials = officials.Where(o => o.CityId == district.CityId).ToList();
            if (cityOfficials.Any())
            {
                var resolver = cityOfficials[Rnd.Next(cityOfficials.Count)];
                var res = new MarkerResolution(marker.Id, resolver.Id, "Информация принята. Службы уведомлены, объект поставлен в график осмотра.");
                
                res.Images.Add(MarkerImage.ForResolution(imgMap["img_result"], res.Id));
                
                context.MarkerResolutions.Add(res);
                marker.ChangeStatus(MarkerStatus.InProgress);

                if (Rnd.NextDouble() > 0.5)
                {
                    marker.ChangeStatus(MarkerStatus.Resolved);
                    var finalRes = new MarkerResolution(marker.Id, resolver.Id, "Работы завершены. Проблема устранена в полном объеме.");
                    finalRes.Images.Add(MarkerImage.ForResolution(imgMap["img_result"], finalRes.Id));
                    context.MarkerResolutions.Add(finalRes);
                }
            }
        }
        context.Markers.Add(marker);
    }
    await context.SaveChangesAsync();
}

    private static Point GetRandomPointInPolygon(Polygon poly)
    {
        var env = poly.EnvelopeInternal;
        while (true)
        {
            double x = env.MinX + Rnd.NextDouble() * env.Width;
            double y = env.MinY + Rnd.NextDouble() * env.Height;
            var p = _geoFactory.CreatePoint(new Coordinate(x, y));
            if (poly.Contains(p)) return p;
        }
        
    }
    private static readonly string[] BioPool = {
        "Активный житель Минска, люблю свой район.",
        "Эко-активист, слежу за чистотой парков.",
        "Урбанист-любитель, топлю за велодорожки.",
        "Мне не все равно, что происходит под окнами.",
        "Официальный представитель службы городского хозяйства.",
        "Интересуюсь историей города и архитектурой.",
        "Волонтер. Помогаю приютам для животных.",
        null 
    };
private static async Task<List<ApplicationUser>> SeedUsers(
    UserManager<ApplicationUser> um, 
    List<City> cities, 
    List<District> dists, 
    Dictionary<string, string> imgMap)
{
    var citySlugs = new Dictionary<string, string> { { "Минск", "Minsk" }, { "Брест", "Brest" }, { "Гродно", "Grodno" } };

    foreach (var city in cities)
    {
        var slug = citySlugs[city.Name];
        var cityDists = dists.Where(d => d.CityId == city.Id).ToList();

        await CreateAcc(um, $"admin_{slug.ToLower()}@echo.by", $"Главный Администратор ({slug})", "Admin", 
            city.Id, cityDists[0].Id, 9999, imgMap.GetValueOrDefault("ava1"), "Курирую техническую часть проекта.");

        for (int i = 1; i <= 2; i++)
        {
            string email = $"official{i}_{slug.ToLower()}@echo.by";
            await CreateAcc(um, email, $"Служба контроля_{i} ({slug})", "Official", 
                city.Id, cityDists[Rnd.Next(cityDists.Count)].Id, 2000, imgMap.GetValueOrDefault("ava2"), "Принимаем и обрабатываем заявки граждан 24/7.");
        }

        await CreateAcc(um, $"mod_{slug.ToLower()}@echo.by", $"Модератор [{slug}]", "Moderator", 
            city.Id, cityDists.Last().Id, 3000, imgMap.GetValueOrDefault("ava3"), "Слежу за соблюдением правил сообщества.");
    }

    for (int i = 0; i < 70; i++)
    {
        var city = cities[Rnd.Next(cities.Count)];
        var district = dists.Where(d => d.CityId == city.Id).OrderBy(_ => Rnd.Next()).First();
        
        string nickname = $"Citizen_{i}"; 
        string email = $"user{i}_{citySlugs[city.Name].ToLower()}@test.by";
        
        await CreateAcc(um, email, nickname, "User", city.Id, district.Id, Rnd.Next(0, 450), null, null);
    }

    return await um.Users.ToListAsync();
}
    private static async Task SeedActivity(AppDbContext context, List<ApplicationUser> users)
    {
        var markers = await context.Markers.ToListAsync();
    
        var addedVotes = new HashSet<(Guid MarkerId, Guid UserId)>();

        foreach (var m in markers)
        {
            int voteCount = Rnd.Next(5, 20);
        
            var potentialVoters = users.OrderBy(_ => Rnd.Next()).Take(voteCount).ToList();

            foreach (var u in potentialVoters)
            {
                if (!addedVotes.Contains((m.Id, u.Id)))
                {
                    context.Votes.Add(new Vote(m.Id, u.Id, Rnd.NextDouble() > 0.2));
                    addedVotes.Add((m.Id, u.Id));
                }
            }

            if (Rnd.NextDouble() > 0.95)
            {
                var reporter = users[Rnd.Next(users.Count)];
                context.Reports.Add(new Report(
                    m.Id, 
                    reporter.Id, 
                    (ReportReason)Rnd.Next(0, 4), 
                    "Содержимое метки нарушает правила сообщества или содержит недостоверную информацию."
                ));
            }
        }
    
        await context.SaveChangesAsync();
    }

    private static async Task<ApplicationUser> CreateAcc(
        UserManager<ApplicationUser> um, 
        string email, 
        string nickname, 
        string role, 
        Guid cityId, 
        Guid dId, 
        int pts, 
        string? ava,
        string? bio = null)
    {
        var user = await um.FindByEmailAsync(email);
        if (user != null) return user;

        var safeUserName = email.Split('@')[0].Replace(".", "").Replace("-", "");

        user = new ApplicationUser 
        { 
            UserName = safeUserName, 
            Email = email, 
            Name = nickname,
            Bio = bio ?? BioPool[Rnd.Next(BioPool.Length)],
            CityId = cityId, 
            DistrictId = dId, 
            Points = pts, 
            AvatarUrl = ava, 
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow.AddDays(-Rnd.Next(10, 200))
        };

        var result = await um.CreateAsync(user, CommonPass);
        if (result.Succeeded)
        {
            await um.AddToRoleAsync(user, role);
        }

        return user;
    }
    
    private static Dictionary<string, string> PrepareImages(IWebHostEnvironment env)
    {
        return new Dictionary<string, string> {
            { "ava1", "https://api.dicebear.com/7.x/avataaars/svg?seed=Admin&backgroundColor=b6e3f4" },
            { "ava2", "https://api.dicebear.com/7.x/avataaars/svg?seed=Official&backgroundColor=ffdfbf" },
            { "ava3", "https://api.dicebear.com/7.x/avataaars/svg?seed=Moderator&backgroundColor=c0aede" },
            { "img1", "https://loremflickr.com/800/600/city,street,architecture?lock=11" },
            { "img2", "https://loremflickr.com/800/600/road,damage,asphalt?lock=22" },
            { "img3", "https://loremflickr.com/800/600/park,garden,city?lock=33" },
            { "img4", "https://loremflickr.com/800/600/building,urban?lock=44" },
            { "img5", "https://loremflickr.com/800/600/city,lighting?lock=55" },
            { "img_result", "https://loremflickr.com/800/600/renovation,clean,city?lock=99" }
        };
    }
}