namespace EventCentric.Authorization
{
    public interface IAuthProvider
    {
        bool IsAuthorized(string token);

        string GetClientIpAddress();
    }
}
