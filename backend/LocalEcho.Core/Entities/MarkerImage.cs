namespace LocalEcho.Core.Entities;

public class MarkerImage
{
    public Guid Id { get; private set; }
    public string Url { get; private set; } = null!;
    public Guid MarkerId { get; private set; }

    private MarkerImage() { }

    public MarkerImage(string url, Guid markerId)
    {
        Id = Guid.NewGuid();
        Url = url;
        MarkerId = markerId;
    }
}