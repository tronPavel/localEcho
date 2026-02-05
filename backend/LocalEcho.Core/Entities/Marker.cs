namespace LocalEcho.Core.Entities;

public class Marker
{
    public Guid Id { get; private set; }
    public string Title { get; private set; } = null!;
    public GeoPoint Location { get; private set; } = null!;
    public string? Description { get; private set; }
    public MarkerCategory Category { get; private set; }
    public MarkerStatus Status { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public Guid DistrictId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private Marker() { }

    private Marker(string title, GeoPoint location, MarkerCategory category, Guid createdByUserId, Guid districtId, string? description)
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
    }

    public static Marker Create(string title, GeoPoint location, MarkerCategory category, Guid createdByUserId, Guid districtId, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title required");
        if (location == null) throw new ArgumentNullException(nameof(location));

        return new Marker(title, location, category, createdByUserId, districtId, description);
    }

    public void UpdateDescription(string? newDescription)
    {
        newDescription = newDescription?.Trim();
        if (newDescription?.Length > 500) throw new ArgumentException("Description too long");
        if (Description == newDescription) return;
        Description = newDescription;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangeStatus(MarkerStatus newStatus)
    {
        if (!Enum.IsDefined(newStatus)) throw new ArgumentException("Invalid status");
        if (Status == MarkerStatus.Resolved && newStatus == MarkerStatus.Active) throw new InvalidOperationException("Cannot reopen");
        if (Status == newStatus) return;
        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;
    }
}