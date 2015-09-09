using System.Web;
using System.Web.Mvc;

namespace EasyTrade.EmpresasReadModel.Web
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
