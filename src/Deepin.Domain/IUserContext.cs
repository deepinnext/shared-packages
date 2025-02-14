namespace Deepin.Domain;
public interface IUserContext
{
    string UserId { get; }
    string UserAgent { get; }
    string IpAddress { get; }
    string AccessToken { get; }
}

