using LocalEcho.Core.Entities.Identity;
using NetTopologySuite.Geometries;

namespace LocalEcho.Core.Entities;

public class Marker
{
    public Guid Id { get; private set; }
    public string Title { get; private set; } = null!;
    
    public Geometry Location { get; private set; } = null!; 
    public string? Description { get; private set; }
    public MarkerCategory Category { get; private set; }
    public MarkerStatus Status { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public Guid? DistrictId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ScheduledAt { get; private set; } 
    public DateTime? ExpiresAt { get; private set; } 
    public DateTime? UpdatedAt { get; private set; }
    public int Rating { get; private set; } = 0;
    
    public virtual ICollection<MarkerImage> Images { get; private set; } = new List<MarkerImage>();
    public virtual ICollection<MarkerResolution> Resolutions { get; private set; } = new List<MarkerResolution>();
    public ApplicationUser? Creator { get; private set; }
    public bool IsHidden { get; private set; } = false;

    public void Hide() 
    {
        IsHidden = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Restore()
    {
        IsHidden = false;
        UpdatedAt = DateTime.UtcNow;
    }

    private Marker() { }

    private Marker(string title, Geometry location, MarkerCategory category, Guid createdByUserId, Guid? districtId, string? description, DateTime? scheduledAt)
    {
        Id = Guid.NewGuid();
        Title = title.Trim();
        Location = location;
        Category = category;
        CreatedByUserId = createdByUserId != Guid.Empty ? createdByUserId : throw new ArgumentException("Creator required");
        DistrictId = districtId;
        Description = description?.Length > 2000 ? throw new ArgumentException("Description too long") : description?.Trim();
        CreatedAt = DateTime.UtcNow;
        Rating = 0;
        ScheduledAt = scheduledAt;
        Status = category switch
        {
            MarkerCategory.Event => MarkerStatus.Upcoming,
            MarkerCategory.Suggestion => MarkerStatus.Review,
            MarkerCategory.Announcement => MarkerStatus.Current,
            _ => MarkerStatus.Active
        };
        
        if (category == MarkerCategory.Announcement)
        {
            ExpiresAt = CreatedAt.AddDays(30);
        }
    }

    public static Marker Create(string title, Geometry location, MarkerCategory category, 
        Guid createdByUserId, Guid? districtId, string? description = null, DateTime? scheduledAt = null)
    {
        if (category == MarkerCategory.Event && !scheduledAt.HasValue)
            throw new ArgumentException("Для события необходимо указать дату и время.");

        return new Marker(title, location, category, createdByUserId, districtId, description, scheduledAt);
    }

    public void SetExpiresAt(DateTime date) => ExpiresAt = date;

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
    
    public void AddResolution(MarkerResolution resolution)
    {
        Resolutions.Add(resolution);
        UpdatedAt = DateTime.UtcNow;
    }
}