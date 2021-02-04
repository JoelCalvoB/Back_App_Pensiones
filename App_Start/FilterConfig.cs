using System.Web;
using System.Web.Mvc;

namespace backend_pensionesmovil
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
