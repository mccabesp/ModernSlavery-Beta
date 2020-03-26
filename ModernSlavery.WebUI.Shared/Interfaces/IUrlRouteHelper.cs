using System.Threading.Tasks;
using ModernSlavery.WebUI.Shared.Options;

namespace ModernSlavery.WebUI.Shared.Classes
{
    public interface IUrlRouteHelper
    {
        Task<string> Get(string routeName);
        Task<string> Get(UrlRouteOptions.Routes routeType);
        Task Set(string routeName, string url);
        Task Set(UrlRouteOptions.Routes routeType, string url);
    }
}