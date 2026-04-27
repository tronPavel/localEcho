using LocalEcho.Core.Entities.Identity;

namespace LocalEcho.Core.Entities;

public class MarkerResolution
{
    public Guid Id { get; private set; }
    public Guid MarkerId { get; private set; }
    public Guid ResolvedByUserId { get; private set; }
    
    public string Comment { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }

    public virtual Marker Marker { get; private set; } = null!;
    public virtual ICollection<MarkerImage> Images { get; private set; } = new List<MarkerImage>();
    public virtual ApplicationUser ResolvedByUser { get; private set; } = null!; 
    private MarkerResolution() { }

    public MarkerResolution(Guid markerId, Guid resolvedByUserId, string comment)
    {
        MarkerId = markerId;
        ResolvedByUserId = resolvedByUserId;
        Comment = comment.Trim();
        CreatedAt = DateTime.UtcNow;
    }
}