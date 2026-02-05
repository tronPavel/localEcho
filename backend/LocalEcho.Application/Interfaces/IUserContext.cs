namespace LocalEcho.Application.Interfaces;

public interface IUserContext
{
    Guid UserId { get; }
    Guid DistrictId { get; }
    bool IsAuthenticated { get; }
}