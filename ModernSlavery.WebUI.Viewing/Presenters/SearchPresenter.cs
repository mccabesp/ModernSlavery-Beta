using System.Collections.Specialized;
using Microsoft.AspNetCore.Mvc;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Infrastructure.Search;
using ModernSlavery.WebUI.Shared.Interfaces;
using ModernSlavery.WebUI.Viewing.Controllers;
using ModernSlavery.WebUI.Viewing.Models.Search;

namespace ModernSlavery.WebUI.Viewing.Presenters
{

    public interface ISearchPresenter
    {
        SearchViewModel LastSearchResults { get; set; }

        string LastSearchParameters { get; }

        string GetLastSearchUrl();

    }

    public class SearchPresenter : ISearchPresenter
    {

        public SearchPresenter(IHttpSession session, IUrlHelper urlHelper, SearchOptions searchOptions)
        {
            Session = session;
            UrlHelper = urlHelper;
            _searchOptions = searchOptions;
        }

        private readonly SearchOptions _searchOptions;

        /// <summary>
        ///     Returns the relative Url of the last search including querystring
        /// </summary>
        public string GetLastSearchUrl()
        {
            NameValueCollection routeValues = LastSearchParameters?.FromQueryString();
            string actionUrl = UrlHelper.Action(nameof(ViewingController.SearchResults), "Viewing");
            string routeQuery = routeValues?.ToQueryString(true);
            return $"{actionUrl}?{routeQuery}";
        }

        #region Dependencies

        public IHttpSession Session { get; }

        public IUrlHelper UrlHelper { get; }

        public bool CacheSearchResults { get; }

        #endregion

        #region Properties

        public SearchViewModel LastSearchResults
        {
            get => CacheSearchResults ? Session.Get<SearchViewModel>("LastSearchResults") : null;
            set
            {
                if (CacheSearchResults)
                {
                    if (value == null || value.Employers == null || value.Employers.Results == null || value.Employers.Results.Count == 0)
                    {
                        Session.Remove("LastSearchResults");
                    }
                    else
                    {
                        Session["LastSearchResults"] = value;
                    }
                }
            }
        }

        /// <summary>
        ///     last search querystring
        /// </summary>
        public string LastSearchParameters
        {
            get => Session["LastSearchParameters"] as string;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    Session.Remove("LastSearchParameters");
                }
                else
                {
                    Session["LastSearchParameters"] = value;
                }
            }
        }

        #endregion

    }


}
