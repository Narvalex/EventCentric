using System.Web;

namespace EventCentric.Authorization
{
    public static class ClientIp
    {
        /// <summary>
        /// Gets the client ip adress. 
        /// More info: http://stackoverflow.com/questions/735350/how-to-get-a-users-client-ip-address-in-asp-net
        /// </summary>
        public static string GetAddress()
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
    }
}
