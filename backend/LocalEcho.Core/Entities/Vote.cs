namespace LocalEcho.Core.Entities;

public class Vote
{
    public Guid MarkerId { get; private set; }
    public Guid UserId { get; private set; }
    public bool IsUpvote { get; private set; }

    private Vote() { }

    public Vote(Guid markerId, Guid userId, bool isUpvote)
    {
        MarkerId = markerId;
        UserId = userId;
        IsUpvote = isUpvote;
    }

    public void ChangeType(bool isUpvote)
    {
        IsUpvote = isUpvote;
    }
}