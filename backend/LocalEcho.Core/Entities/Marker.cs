namespace LocalEcho.Core.Entities;

public class Marker
{
    public Guid Id { get; private set; }
    public string Title { get; private set; } = null!;
    public GeoPoint Location { get; private set; } = null!;
    public string? Description { get; private set; }
    public MarkerCategory Category { get; private set; }
    public MarkerStatus Status { get; private set; }
    
    // public Guid CreatorId { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private Marker() { }

    private Marker(string title, GeoPoint location, MarkerCategory category, string? description)
    {
        Id = Guid.NewGuid();
        Title = title.Trim();
        Location = location;
        Category = category;
        Description = description?.Length > 500 
            ? throw new ArgumentException("Description too long", nameof(description))
            : description?.Trim();
        Status = MarkerStatus.Active;
        CreatedAt = DateTime.UtcNow;
    }

    public static Marker Create(string title, GeoPoint location, MarkerCategory category, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required", nameof(title));
        if (location is null) throw new ArgumentNullException(nameof(location));

        return new Marker(title, location, category, description);
    }

    // todo параметр Guid currentUserId + проверка прав
    public void UpdateDescription(string? newDescription)
    {
        newDescription = newDescription?.Trim();
        if (newDescription?.Length > 500)
            throw new ArgumentException("Description cannot exceed 500 characters");

        if (Description == newDescription) return;

        Description = newDescription;
        UpdatedAt = DateTime.UtcNow;
    }

    // todo параметр Guid currentUserId + проверка прав (или отдельно для модераторов)
    public void ChangeStatus(MarkerStatus newStatus)
    {
        if (!Enum.IsDefined(newStatus))
            throw new ArgumentException("Invalid status", nameof(newStatus));

        if (Status == MarkerStatus.Resolved && newStatus == MarkerStatus.Active)
            throw new InvalidOperationException("Cannot reopen a resolved marker");

        if (Status == newStatus) return;

        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;
    }
}