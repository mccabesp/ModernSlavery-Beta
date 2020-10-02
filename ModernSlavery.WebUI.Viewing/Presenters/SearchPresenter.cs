using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ModernSlavery.Core.Extensions;
using ModernSlavery.WebUI.Shared.Interfaces;
using ModernSlavery.WebUI.Viewing.Controllers;
using ModernSlavery.WebUI.Viewing.Models;
using System.Linq;

namespace ModernSlavery.WebUI.Viewing.Presenters
{
    public interface ISearchPresenter
    {
        void CacheCurrentSearchUrl();

        string GetLastSearchUrl();
    }

    public class SearchPresenter : ISearchPresenter
    {
        #region Dependencies

        public IHttpSession Session { get; }

        public IUrlHelper UrlHelper { get; }

        public IHttpContextAccessor HttpContextAccessor { get; }

        #endregion

        public SearchPresenter(IHttpSession session,
            IHttpContextAccessor httpContextAccessor,
            IUrlHelper urlHelper)
        {
            Session = session;
            UrlHelper = urlHelper;
            HttpContextAccessor = httpContextAccessor;
        }

        const string SEARCH_KEY = "Last_Search";

        public void CacheCurrentSearchUrl()
        {
            var query = HttpContextAccessor.HttpContext.Request.QueryString;
            // We should always use the search url as that appropriately handles 
            // the client having JS disabled or enabled
            var action = UrlHelper.Action(nameof(ViewingController.SearchResults), "Viewing");
            var url = action + query.Value;

            Session[SEARCH_KEY] = url;
        }

        /// <summary>
        ///     Returns the relative Url of the last search including querystring
        /// </summary>
        public string GetLastSearchUrl()
        {
            if (Session.Keys.Contains(SEARCH_KEY))
                return Session[SEARCH_KEY].ToString();

            return UrlHelper.Action(nameof(ViewingController.SearchResults), "Viewing");
        }
    }
}