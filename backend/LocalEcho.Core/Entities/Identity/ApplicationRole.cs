using Microsoft.AspNetCore.Identity;

namespace LocalEcho.Core.Entities.Identity;

public class ApplicationRole : IdentityRole<Guid>
{
    public string? Description { get; set; }
}