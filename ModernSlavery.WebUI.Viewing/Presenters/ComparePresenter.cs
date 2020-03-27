using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.SharedKernel;
using ModernSlavery.WebUI.Shared.Classes.Cookies;
using ModernSlavery.WebUI.Shared.Interfaces;
using ModernSlavery.WebUI.Viewing.Classes;

namespace ModernSlavery.WebUI.Viewing.Presenters
{
    public interface IComparePresenter
    {
        Lazy<SessionList<string>> ComparedEmployers { get; }

        int MaxCompareBasketShareCount { get; }

        int MaxCompareBasketCount { get; }

        string LastComparedEmployerList { get; set; }

        string SortColumn { get; set; }

        bool SortAscending { get; set; }

        int BasketItemCount { get; }

        void AddToBasket(string encEmployerId);

        void AddRangeToBasket(string[] encEmployerIds);

        void RemoveFromBasket(string encEmployerId);

        void ClearBasket();

        void LoadComparedEmployersFromCookie();

        void SaveComparedEmployersToCookie(HttpRequest request);

        bool BasketContains(params string[] encEmployerIds);
    }

    public class ComparePresenter : IComparePresenter
    {
        public ComparePresenter(IOptionsSnapshot<ViewingOptions> options, IHttpContextAccessor httpContext,
            IHttpSession session)
        {
            Options = options;
            HttpContext = httpContext.HttpContext;
            Session = session;
            ComparedEmployers = new Lazy<SessionList<string>>(CreateCompareSessionList(Session));
        }

        public IOptionsSnapshot<ViewingOptions> Options { get; }

        public void LoadComparedEmployersFromCookie()
        {
            var value = HttpContext.GetRequestCookieValue(CookieNames.LastCompareQuery);

            if (string.IsNullOrWhiteSpace(value)) return;

            var employerIds = value.SplitI(",");

            ClearBasket();

            if (employerIds.Any()) AddRangeToBasket(employerIds);
        }

        public void SaveComparedEmployersToCookie(HttpRequest request)
        {
            var employerIds = ComparedEmployers.Value.ToList();

            var cookieSettings = CookieHelper.GetCookieSettingsCookie(request);
            if (cookieSettings.RememberSettings)
                //Save into the cookie
                HttpContext.SetResponseCookie(
                    CookieNames.LastCompareQuery,
                    employerIds.ToDelimitedString(),
                    VirtualDateTime.Now.AddMonths(1),
                    secure: true);
        }

        private SessionList<string> CreateCompareSessionList(IHttpSession session)
        {
            return new SessionList<string>(
                session,
                nameof(ComparePresenter),
                nameof(ComparedEmployers));
        }

        #region Dependencies

        public HttpContext HttpContext { get; }

        public IHttpSession Session { get; }

        #endregion

        #region Properties

        public Lazy<SessionList<string>> ComparedEmployers { get; }

        public string LastComparedEmployerList
        {
            get => Session["LastComparedEmployerList"].ToStringOrNull();
            set => Session["LastComparedEmployerList"] = value;
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

        public int BasketItemCount => ComparedEmployers.Value.Count;

        public int MaxCompareBasketCount => Options.Value.MaxCompareBasketCount;

        public int MaxCompareBasketShareCount => Options.Value.MaxCompareBasketShareCount;

        #endregion

        #region Basket Methods

        public void AddToBasket(string encEmployerId)
        {
            var newBasketCount = ComparedEmployers.Value.Count + 1;
            if (newBasketCount <= MaxCompareBasketCount) ComparedEmployers.Value.Add(encEmployerId);
        }

        public void AddRangeToBasket(string[] encEmployerIds)
        {
            var newBasketCount = ComparedEmployers.Value.Count + encEmployerIds.Length;
            if (newBasketCount <= MaxCompareBasketCount) ComparedEmployers.Value.Add(encEmployerIds);
        }

        public void RemoveFromBasket(string encEmployerId)
        {
            ComparedEmployers.Value.Remove(encEmployerId);
        }

        public void ClearBasket()
        {
            ComparedEmployers.Value.Clear();
        }

        public bool BasketContains(params string[] encEmployerIds)
        {
            return ComparedEmployers.Value.Contains(encEmployerIds);
        }

        #endregion
    }
}