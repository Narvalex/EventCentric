namespace EventCentric.Authorization
{
    public class AuthorizeEventSourcingAttribute : AuthorizeBaseAttribute
    {
        private static string _token;

        protected override bool IsAuthorized(string token)
         => token == _token ? true : false;

        public static void Configure(string token)
         => _token = $"Bearer {token}";
    }
}
