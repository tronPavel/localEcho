namespace LocalEcho.Core.Entities;

public class MarkerImage
{
    public Guid Id { get; private set; }
    public string Url { get; private set; } = null!;
    
    public Guid? MarkerId { get; private set; }
    public Guid? MarkerResolutionId { get; private set; }

    private MarkerImage() { }

    public MarkerImage(string url, Guid? markerId = null, Guid? resolutionId = null)
    {
        Id = Guid.NewGuid();
        Url = url;
        MarkerId = markerId;
        MarkerResolutionId = resolutionId;
    }

    public static MarkerImage ForMarker(string url, Guid markerId) 
        => new MarkerImage(url, markerId: markerId);

    public static MarkerImage ForResolution(string url, Guid resolutionId) 
        => new MarkerImage(url, resolutionId: resolutionId);
}