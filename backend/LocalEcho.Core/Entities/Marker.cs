using LocalEcho.Core.Entities.Identity;
using NetTopologySuite.Geometries;

namespace LocalEcho.Core.Entities;

public class Marker
{
    public Guid Id { get; private set; }
    public string Title { get; private set; } = null!;
    
    public Point Location { get; private set; } = null!;
    
    public string? Description { get; private set; }
    public MarkerCategory Category { get; private set; }
    public MarkerStatus Status { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public Guid DistrictId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public int Rating { get; private set; } = 0;
    
    public virtual ICollection<MarkerImage> Images { get; private set; } = new List<MarkerImage>();
    public virtual MarkerResolution? Resolution { get; private set; }
    
    public ApplicationUser? Creator { get; private set; }

    private Marker() { }

    private Marker(string title, Point location, MarkerCategory category, Guid createdByUserId, Guid districtId, string? description)
    {
        Id = Guid.NewGuid();
        Title = title.Trim();
        Location = location;
        Category = category;
        CreatedByUserId = createdByUserId != Guid.Empty ? createdByUserId : throw new ArgumentException("Creator required");
        DistrictId = districtId != Guid.Empty ? districtId : throw new ArgumentException("District required");
        Description = description?.Length > 500 ? throw new ArgumentException("Description too long") : description?.Trim();
        Status = MarkerStatus.Active;
        CreatedAt = DateTime.UtcNow;
        Rating = 0;
    }

    public static Marker Create(string title, Point location, MarkerCategory category, Guid createdByUserId, Guid districtId, string? description = null, string? imageUrl = null)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title required");
        if (location == null) throw new ArgumentNullException(nameof(location));
        
        return new Marker(title, location, category, createdByUserId, districtId, description);
    }

    public void UpdateContent(string title, string? description)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Заголовок не может быть пустым");
        Title = title.Trim();
        Description = description?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangeStatus(MarkerStatus newStatus)
    {
        if (Status == MarkerStatus.Resolved && newStatus != MarkerStatus.Resolved) 
            throw new InvalidOperationException("Нельзя открыть заново уже решенную проблему.");
        
        if (Status == newStatus) return;
        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;
    }

    
    public void UpdateRating(int newRating) => Rating = newRating;
    
    public void SetResolution(MarkerResolution resolution)
    {
        if (Status == MarkerStatus.Resolved) 
            throw new InvalidOperationException("Метка уже имеет решение.");
            
        Resolution = resolution;
        ChangeStatus(MarkerStatus.Resolved);
    }
}