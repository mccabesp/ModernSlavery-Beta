using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using ModernSlavery.Core;
using ModernSlavery.Core.Extensions;
using ModernSlavery.WebUI.Shared.Classes.Cookies;
using ModernSlavery.WebUI.Shared.Interfaces;
using ModernSlavery.WebUI.Viewing.Classes;

namespace ModernSlavery.WebUI.Viewing.Presenters
{
    public interface IComparePresenter
    {
        Lazy<SessionList<string>> ComparedOrganisations { get; }

        int MaxCompareBasketShareCount { get; }

        int MaxCompareBasketCount { get; }

        string LastComparedOrganisationList { get; set; }

        string SortColumn { get; set; }

        bool SortAscending { get; set; }

        int BasketItemCount { get; }

        void AddToBasket(string encOrganisationId);

        void AddRangeToBasket(string[] encOrganisationIds);

        void RemoveFromBasket(string encOrganisationId);

        void ClearBasket();

        void LoadComparedOrganisationsFromCookie();

        void SaveComparedOrganisationsToCookie(HttpRequest request);

        bool BasketContains(params string[] encOrganisationIds);
    }

    public class ComparePresenter : IComparePresenter
    {
        public ComparePresenter(IOptionsSnapshot<ViewingOptions> options, IHttpContextAccessor httpContext,
            IHttpSession session)
        {
            Options = options;
            HttpContext = httpContext.HttpContext;
            Session = session;
            ComparedOrganisations = new Lazy<SessionList<string>>(CreateCompareSessionList(Session));
        }

        public IOptionsSnapshot<ViewingOptions> Options { get; }

        public void LoadComparedOrganisationsFromCookie()
        {
            var value = HttpContext.GetRequestCookieValue(CookieNames.LastCompareQuery);

            if (string.IsNullOrWhiteSpace(value)) return;

            var organisationIds = value.SplitI(",");

            ClearBasket();

            if (organisationIds.Any()) AddRangeToBasket(organisationIds);
        }

        public void SaveComparedOrganisationsToCookie(HttpRequest request)
        {
            var organisationIds = ComparedOrganisations.Value.ToList();

            var cookieSettings = CookieHelper.GetCookieSettingsCookie(request);
            if (cookieSettings.RememberSettings)
                //Save into the cookie
                HttpContext.SetResponseCookie(
                    CookieNames.LastCompareQuery,
                    organisationIds.ToDelimitedString(),
                    VirtualDateTime.Now.AddMonths(1),
                    secure: true);
        }

        private SessionList<string> CreateCompareSessionList(IHttpSession session)
        {
            return new SessionList<string>(
                session,
                nameof(ComparePresenter),
                nameof(ComparedOrganisations));
        }

        #region Dependencies

        public HttpContext HttpContext { get; }

        public IHttpSession Session { get; }

        #endregion

        #region Properties

        public Lazy<SessionList<string>> ComparedOrganisations { get; }

        public string LastComparedOrganisationList
        {
            get => Session["LastComparedOrganisationList"].ToStringOrNull();
            set => Session["LastComparedOrganisationList"] = value;
        }

        public string SortColumn
        {
            get => Session["SortColumn"].ToStringOrNull();
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    Session.Remove("SortColumn");
                else
                    Session["SortColumn"] = value;
            }
        }

        public bool SortAscending
        {
            get => Session["SortAscending"].ToBoolean(true);
            set => Session["SortAscending"] = value;
        }

        public int BasketItemCount => ComparedOrganisations.Value.Count;

        public int MaxCompareBasketCount => Options.Value.MaxCompareBasketCount;

        public int MaxCompareBasketShareCount => Options.Value.MaxCompareBasketShareCount;

        #endregion

        #region Basket Methods

        public void AddToBasket(string encOrganisationId)
        {
            var newBasketCount = ComparedOrganisations.Value.Count + 1;
            if (newBasketCount <= MaxCompareBasketCount) ComparedOrganisations.Value.Add(encOrganisationId);
        }

        public void AddRangeToBasket(string[] encOrganisationIds)
        {
            var newBasketCount = ComparedOrganisations.Value.Count + encOrganisationIds.Length;
            if (newBasketCount <= MaxCompareBasketCount) ComparedOrganisations.Value.Add(encOrganisationIds);
        }

        public void RemoveFromBasket(string encOrganisationId)
        {
            ComparedOrganisations.Value.Remove(encOrganisationId);
        }

        public void ClearBasket()
        {
            ComparedOrganisations.Value.Clear();
        }

        public bool BasketContains(params string[] encOrganisationIds)
        {
            return ComparedOrganisations.Value.Contains(encOrganisationIds);
        }

        #endregion
    }
}