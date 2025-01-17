namespace Deepin.ServiceDefaults.Services;
public interface IUserContext
{
    string UserId { get; }
    string UserAgent { get; }
    string IpAddress { get; }
}
