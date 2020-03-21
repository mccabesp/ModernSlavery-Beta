using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ModernSlavery.Infrastructure
{
    public class PostcodesChecker : IPostcodeChecker
    {
        public async Task<bool> IsValidPostcode(string postcode)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.BaseAddress = new Uri("https://api.postcodes.io");

                    var path = $"/postcodes/{postcode}/validate";

                    var response = await httpClient.GetAsync(path);

                    if (response.IsSuccessStatusCode)
                    {
                        var bodyString = await response.Content.ReadAsStringAsync();
                        var body = JsonConvert.DeserializeObject<PostcodesIoApiValidateResponse>(bodyString);
                        return body.result;
                    }

                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    internal class PostcodesIoApiValidateResponse
    {
        public bool result { get; set; }
    }
}