using System.Web;

namespace EventCentric.Authorization
{
    public interface IIdentity
    {
        bool IsAuthorized(string token);

        string GetClientIpAddress();
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

        /// <summary>
        /// Gets the client ip adress. 
        /// More info: http://stackoverflow.com/questions/735350/how-to-get-a-users-client-ip-address-in-asp-net
        /// </summary>
        public string GetClientIpAddress()
        {
            var context = HttpContext.Current;
            var ipAddress = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (!string.IsNullOrEmpty(ipAddress))
            {
                var addresses = ipAddress.Split(',');
                if (addresses.Length != 0)
                    return addresses[0];
            }

            return context.Request.ServerVariables["REMOTE_ADDR"];
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
