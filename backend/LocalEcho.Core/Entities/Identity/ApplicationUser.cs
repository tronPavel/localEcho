using Microsoft.AspNetCore.Identity;

namespace LocalEcho.Core.Entities.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public string? Name { get; set; }
    public string? AvatarUrl { get; set; }
    public Guid? DistrictId { get; set; }
    public string? HomeAddress { get; set; }
    public double? HomeLatitude { get; set; }
    public double? HomeLongitude { get; set; }
    public bool IsVerified { get; set; } = false;
    public int Points { get; set; } = 0;
    public DateTime LastSeen { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}