using System.Threading.Tasks;
using ModernSlavery.WebUI.Shared.Options;

namespace ModernSlavery.WebUI.Shared.Classes
{
    public interface IUrlRouteHelper
    {
        Task<string> Get(string routeName, object values = null);
        Task<string> Get(UrlRouteOptions.Routes routeType, object values=null);
        Task Set(string routeName, string url);
        Task Set(UrlRouteOptions.Routes routeType, string url);
    }
}