using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.WebTesting;

namespace ModernSlavery.WebTestPlugins
{
    public class AddressFixPlugin : WebTestPlugin
    {
        // Properties for the plugin.  
        public override void PreRequestDataBinding(object sender, PreRequestDataBindingEventArgs e)
        {

            if (!e.Request.Url.Contains("add-address")) return;
            if (!e.Request.Method.Equals("post", StringComparison.OrdinalIgnoreCase)) return;
            
            var address1 = e.WebTest.Context.ContainsKey("Address1") ? e.WebTest.Context["Address1"]?.ToString() : string.Empty;
            var address2 = e.WebTest.Context.ContainsKey("Address2") ? e.WebTest.Context["Address2"]?.ToString() : string.Empty;
            var city = e.WebTest.Context.ContainsKey("City") ? e.WebTest.Context["City"]?.ToString() : string.Empty;
            var county = e.WebTest.Context.ContainsKey("County") ? e.WebTest.Context["County"]?.ToString() : string.Empty;
            var country = e.WebTest.Context.ContainsKey("Country") ? e.WebTest.Context["Country"]?.ToString() : string.Empty;
            var postcode = e.WebTest.Context.ContainsKey("Postcode") ? e.WebTest.Context["Postcode"]?.ToString() : string.Empty;

            var addressParts = new List<string>();
            if (!string.IsNullOrWhiteSpace(address1)) addressParts.Add(address1);
            if (!string.IsNullOrWhiteSpace(address2)) addressParts.Add(address2); 
            if (!string.IsNullOrWhiteSpace(city)) addressParts.Add(city);
            if (!string.IsNullOrWhiteSpace(county)) addressParts.Add(county);

            address1 = addressParts.Count > 0 ? addressParts[0] : string.Empty;
            if (addressParts.Count>0)addressParts.RemoveAt(0);
            address2 = addressParts.Count > 0 ? addressParts[0] : string.Empty;
            if (addressParts.Count > 0) addressParts.RemoveAt(0);
            city = addressParts.Count > 0 ? addressParts[0] : string.Empty;
            if (addressParts.Count > 0) addressParts.RemoveAt(0);
            county = addressParts.Count > 0 ? addressParts[0] : string.Empty;
            if (addressParts.Count > 0) addressParts.RemoveAt(0);

            e.WebTest.Context["Address1"] = !string.IsNullOrWhiteSpace(address1) ? address1 : "Address 1";
            e.WebTest.Context["Address2"] = address2;
            e.WebTest.Context["City"] = !string.IsNullOrWhiteSpace(city) ? city : "City 1";
            e.WebTest.Context["County"] = county;
            e.WebTest.Context["Country"]=!string.IsNullOrWhiteSpace(country) ?  country :  "UK";
            e.WebTest.Context["Postcode"] = !string.IsNullOrWhiteSpace(postcode) ? postcode : "POST CODE1";

            base.PreRequestDataBinding(sender, e);

        }
    }
}