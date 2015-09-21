namespace EventCentric.Authorization
{
    public interface IIdentity
    {
        bool IsAuthorized(string token);
    }

    public class Identity : IIdentity
    {
        private static string _token;

        private Identity()
        { }

        public bool IsAuthorized(string token)
        {
            return token == _token ? true : false;
        }

        public static void Configure(string token)
        {
            _token = $"Bearer {token}";
        }

        public static IIdentity Resolve()
        {
            return new Identity();
        }
    }
}
