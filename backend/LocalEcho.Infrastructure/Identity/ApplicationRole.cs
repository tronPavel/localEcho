using Microsoft.AspNetCore.Identity;

namespace LocalEcho.Infrastructure.Identity;

public class ApplicationRole : IdentityRole<Guid>
{
    public string? Description { get; set; }
}