namespace EventCentric.Authorization
{
    public class SimpleAuthProvider : AuthProviderBase
    {
        private static string _token;

        private SimpleAuthProvider()
        { }

        public override bool IsAuthorized(string token)
        {
            return token == _token ? true : false;
        }

        public static void Configure(string token)
        {
            _token = $"Bearer {token}";
        }
    }
}
