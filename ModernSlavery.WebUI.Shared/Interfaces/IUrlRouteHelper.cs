using ModernSlavery.WebUI.Shared.Options;

namespace ModernSlavery.WebUI.Shared.Interfaces
{
    public interface IUrlRouteHelper
    {
        string Get(string routeName, object values = null);
        string Get(UrlRouteOptions.Routes routeType, object values = null);
    }
}