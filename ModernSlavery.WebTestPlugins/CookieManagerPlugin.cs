using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.VisualStudio.TestTools.WebTesting;

namespace ModernSlavery.WebTestPlugins
{
    public class CookieManagerPlugin : WebTestPlugin
    {
        public override void PreWebTest(object sender, PreWebTestEventArgs e)
        {
            if (!e.WebTest.Context.ContainsKey("IsNewContext"))
            {
                e.WebTest.Context.CookieContainer = new System.Net.CookieContainer();
                SaveAllCookies(e.WebTest.Context,new Dictionary<string, Dictionary<string, Cookie>>());
            }
            base.PreWebTest(sender, e);
        }

        public override void PreRequest(object sender, PreRequestEventArgs e)
        {
            var url = e.Request.Url;
            var uri = new Uri(url);

            var allCookies = LoadAllCookies(e.WebTest.Context);
            var cookies = allCookies.ContainsKey(uri.Authority) ? allCookies[uri.Authority] : new Dictionary<string, Cookie>();
            
            var collection = cookies;
            if (collection != null)
                foreach (var key in collection.Keys)
                {
                    if (collection[key].Expired || collection[key].Expires<DateTime.Now || !uri.AbsolutePath.StartsWith(collection[key].Path, StringComparison.OrdinalIgnoreCase)) continue;

                    var cookie = e.Request.Cookies[key];
                    if (cookie == null)
                        e.Request.Cookies.Add(new Cookie(key, collection[key].Value));
                    else if (e.Request.Cookies[cookie.Name].Value != cookie.Value)
                        e.Request.Cookies[cookie.Name].Value = collection[key].Value;
                }
        }

        public override void PostRequest(object sender, PostRequestEventArgs e)
        {
            base.PostRequest(sender, e);

            var url = e.Request.Url;
            var uri = new Uri(url);

            var allCookies = LoadAllCookies(e.WebTest.Context);
            var cookies = allCookies.ContainsKey(uri.Authority) ? allCookies[uri.Authority] : new Dictionary<string, Cookie>();

            if (!e.ResponseExists) return;

            var collection = e.Response.Cookies;
            if (collection != null)
                for (int i=0;i< collection.Count;i++)
                {
                    var cookie = collection[i];
                    if (!cookie.Expired && cookie.Expires >= DateTime.Now)
                        cookies[cookie.Name] = cookie;
                    else if (cookies.ContainsKey(cookie.Name))
                        cookies.Remove(cookie.Name);
                }


            collection = e.WebTest.Context.CookieContainer.GetCookies(uri);
            if (collection!=null)
                for (int i = 0; i < collection.Count; i++)
                {
                    var cookie = collection[i];
                    if (!cookie.Expired && cookie.Expires >= DateTime.Now)
                        cookies[cookie.Name] = cookie;
                    else if (cookies.ContainsKey(cookie.Name))
                        cookies.Remove(cookie.Name);
                }

            allCookies[uri.Authority] = cookies;

            SaveAllCookies(e.WebTest.Context, allCookies);
        }

        public override void PostPage(object sender, PostPageEventArgs e)
        {
            base.PostPage(sender, e);
            if (!string.IsNullOrWhiteSpace(e.Response?.ResponseUri?.Scheme)) e.WebTest.Context["LastUrl"] = e.Response.ResponseUri;
            e.WebTest.Context["IsNewContext"] = false;
        }

        Dictionary<string, Dictionary<string, Cookie>> LoadAllCookies(WebTestContext context)
        {

            var allCookies= new Dictionary<string, Dictionary<string, Cookie>>();
            if (context.ContainsKey("AllCookies"))
                allCookies=context["AllCookies"] as Dictionary<string, Dictionary<string, Cookie>>;
            else
                context["AllCookies"] = allCookies;

            return allCookies;
        }

        void SaveAllCookies(WebTestContext context,Dictionary<string, Dictionary<string, Cookie>> allCookies)
        {
            context["AllCookies"] = allCookies;
        }

    }
}