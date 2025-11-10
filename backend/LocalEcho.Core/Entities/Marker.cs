namespace LocalEcho.Core.Entities;

public class Marker
{
    public Guid Id { get; private set; }
    public string Title { get; private set; } 
    public GeoPoint Location{ get; private set; }
    public string? Description { get; private set; } 
    public MarkerCategory Category { get; private set; } 
    public MarkerStatus Status { get; private set; } 
    //public Guid CreatorId { get; private set; } 
    public DateTime? UpdatedAt { get; private set; }
    
    public DateTime CreatedAt { get; private set; }

    public Marker(string title, GeoPoint location, MarkerCategory category, /*Guid creatorId,*/ string? description = null)
    {
        
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty", nameof(title));
        
        if (!Enum.IsDefined(typeof(MarkerCategory), category))
            throw new ArgumentException("Invalid category", nameof(category));

        //if (creatorId == Guid.Empty)
        //throw new ArgumentException("CreatorId cannot be empty", nameof(creatorId));

        if (description != null && description.Length > 500)
            throw new ArgumentException("Description cannot exceed 500 characters", nameof(description));

        Id = Guid.NewGuid();
        Title = title;
        Location = location;
        Description = description;
        Category = category;
        //CreatorId = creatorId;
        Status = MarkerStatus.Active; 
        CreatedAt = DateTime.UtcNow;
    }

    public void ChangeStatus(MarkerStatus newStatus)
    {
        if (!Enum.IsDefined(typeof(MarkerStatus), newStatus))
            throw new ArgumentException("Invalid status", nameof(newStatus));

        if (Status == MarkerStatus.Resolved && newStatus == MarkerStatus.Active)
            throw new InvalidOperationException("Cannot revert Resolved to Active");

        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDescription(string? newDescription)
    {
        if (newDescription != null && newDescription.Length > 500)
            throw new ArgumentException("Description cannot exceed 500 characters", nameof(newDescription));

        Description = newDescription;
        UpdatedAt = DateTime.UtcNow;
    }
}