using Microsoft.AspNetCore.Identity;
using NetTopologySuite.Geometries;

namespace LocalEcho.Core.Entities.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public string? Name { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; } 
    public Guid? DistrictId { get; set; }
    public Guid? CityId { get; set; } 
    public string? HomeAddress { get; set; }
    public Point? HomeLocation { get; set; } 
    public int Points { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}