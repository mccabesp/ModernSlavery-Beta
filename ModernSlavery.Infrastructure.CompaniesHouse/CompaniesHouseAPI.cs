using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using ModernSlavery.Core;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.Core.Models.CompaniesHouse;
using ModernSlavery.Core.Options;
using Newtonsoft.Json;
using Polly;
using Polly.Extensions.Http;

namespace ModernSlavery.Infrastructure.CompaniesHouse
{
    public class CompaniesHouseAPI : ICompaniesHouseAPI
    {
        private readonly CompaniesHouseOptions _companiesHouseOptions;
        private readonly HttpClient _httpClient;
        private readonly string[] _apiKeys;
        public RetryPolicyTypes RetryPolicy { get; set; } = RetryPolicyTypes.None;

        public CompaniesHouseAPI(CompaniesHouseOptions companiesHouseOptions, HttpClient httpClient)
        {
            _companiesHouseOptions = companiesHouseOptions ?? throw new ArgumentNullException("You must provide the companies house options",nameof(CompaniesHouseOptions));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _apiKeys = _companiesHouseOptions.GetApiKeys();
            if (_apiKeys.Length==0)throw new ArgumentNullException(nameof(companiesHouseOptions.ApiKey));
        }

        public async Task<PagedResult<OrganisationRecord>> SearchOrganisationsAsync(string searchText, int page, int pageSize, int maxRecords)
        {
            if (searchText.IsNumber()) searchText = searchText.PadLeft(8, '0');

            SetApiKey();

            //Get the first page of results and the total records, number of pages, and page size
            var page1task = SearchCompaniesAsync(searchText, 1, pageSize);
            await page1task.ConfigureAwait(false);

            //Calculate the maximum page size
            int maxPages = (int) Math.Ceiling((double)maxRecords / page1task.Result.PageSize);
            maxPages = page1task.Result.ActualPageCount > maxPages ? maxPages : page1task.Result.ActualPageCount;

            //Add a task for ll pages from 2 upwards to maxpages
            var tasks = new List<Task<PagedResult<OrganisationRecord>>>();
            for (var subPage = 2; subPage <= maxPages; subPage++)
                tasks.Add(SearchCompaniesAsync(searchText, subPage, page1task.Result.PageSize));

            //Wait for all the tasks to complete
            if (tasks.Count > 0)await Task.WhenAll(tasks).ConfigureAwait(false);

            //Add page 1 to the list of completed tasks
            tasks.Insert(0, page1task);

            var organisationsPage = new PagedResult<OrganisationRecord>
            {
                PageSize = pageSize,
                CurrentPage = page,
                Results = new List<OrganisationRecord>()
            };

            //Merge the results from each page into a single page of results
            foreach (var task in tasks) organisationsPage.Results.AddRange(task.Result.Results);

            //Get the toal number of records
            organisationsPage.ActualRecordTotal = page1task.Result.ActualRecordTotal;
            organisationsPage.VirtualRecordTotal = page1task.Result.VirtualRecordTotal;

            return organisationsPage;
        }

        public async Task<string> GetSicCodesAsync(string companyNumber)
        {
            if (companyNumber.IsNumber()) companyNumber = companyNumber.PadLeft(8, '0');

            var codes = new HashSet<string>();

            var company = await GetCompanyAsync(companyNumber).ConfigureAwait(false);
            if (company == null) return null;

            if (company.SicCodes != null)
                foreach (var code in company.SicCodes)
                    codes.Add(code);

            return codes.ToDelimitedString();
        }

        //Sets the api key to the next random value
        private void SetApiKey()
        {
            var apiKey = _apiKeys[Numeric.Rand(0, _apiKeys.Length - 1)];
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",Convert.ToBase64String(Encoding.ASCII.GetBytes($"{apiKey}:")));
        }

        public async Task<CompaniesHouseCompany> GetCompanyAsync(string companyNumber)
        {
            if (companyNumber.IsNumber()) companyNumber = companyNumber.PadLeft(8, '0');

            // capture any serialization errors
            string json = null;
            try
            {
                SetApiKey();

                var response = await _httpClient.GetAsync($"/company/{companyNumber}").ConfigureAwait(false);
                // Migration to dotnet core work around return status codes until over haul of this API client
                if (response.StatusCode != HttpStatusCode.OK) throw new HttpException(response.StatusCode);

                json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return JsonConvert.DeserializeObject<CompaniesHouseCompany>(json);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"The response from the companies house api returned an invalid json object.\n\nCompanyNumber = {companyNumber}\nResponse = {json}",
                    ex.InnerException ?? ex);
            }
        }

        private async Task<PagedResult<OrganisationRecord>> SearchCompaniesAsync(string searchText, int page, int pageSize)
        {
            var organisationsPage = new PagedResult<OrganisationRecord>
            {
                PageSize = pageSize, CurrentPage = page, Results = new List<OrganisationRecord>()
            };

            var json = await GetCompaniesAsync(searchText, page, organisationsPage.PageSize).ConfigureAwait(false);

            dynamic companies = JsonConvert.DeserializeObject(json);
            if (companies != null)
            {
                organisationsPage.ActualRecordTotal = companies.total_results;
                organisationsPage.VirtualRecordTotal = companies.total_results;
                organisationsPage.PageSize = companies.items_per_page;
                if (organisationsPage.ActualRecordTotal > 0)
                    foreach (var company in companies.items)
                    {
                        var organisation = new OrganisationRecord();
                        organisation.OrganisationName = company.title;
                        organisation.NameSource = "CoHo";
                        organisation.CompanyNumber = company.company_number;
                        if (organisation.CompanyNumber.IsNumber())
                            organisation.CompanyNumber = organisation.CompanyNumber.PadLeft(8, '0');

                        var dateOfCessation = ((string) company?.date_of_cessation).ToDateTime();
                        if (dateOfCessation > DateTime.MinValue) organisation.DateOfCessation = dateOfCessation;

                        if (company.address != null)
                        {
                            string premises = null,
                                addressLine1 = null,
                                addressLine2 = null,
                                addressLine3 = null,
                                city = null,
                                county = null,
                                country = null,
                                postalCode = null,
                                poBox = null;
                            if (company.address.premises != null)
                                premises = ((string) company.address.premises).CorrectNull().TrimI(", ");

                            if (company.address.care_of != null)
                                addressLine1 = ((string) company.address.care_of).CorrectNull().TrimI(", ");

                            if (company.address.address_line_1 != null)
                                addressLine2 = ((string) company.address.address_line_1).CorrectNull().TrimI(", ");

                            if (!string.IsNullOrWhiteSpace(premises)) addressLine2 = premises + ", " + addressLine2;

                            if (company.address.address_line_2 != null)
                                addressLine3 = ((string) company.address.address_line_2).CorrectNull().TrimI(", ");

                            if (company.address.locality != null)
                                city = ((string) company.address.locality).CorrectNull().TrimI(", ");

                            if (company.address.region != null)
                                county = ((string) company.address.region).CorrectNull().TrimI(", ");

                            if (company.address.country != null)
                                country = ((string) company.address.country).CorrectNull().TrimI(", ");

                            if (company.address.postal_code != null)
                                postalCode = ((string) company.address.postal_code).CorrectNull().TrimI(", ");

                            if (company.address.po_box != null)
                                poBox = ((string) company.address.po_box).CorrectNull().TrimI(", ");

                            var addresses = new List<string>();
                            if (!string.IsNullOrWhiteSpace(addressLine1)) addresses.Add(addressLine1);

                            if (!string.IsNullOrWhiteSpace(addressLine2) && !addresses.ContainsI(addressLine2))
                                addresses.Add(addressLine2);

                            if (!string.IsNullOrWhiteSpace(addressLine3) && !addresses.ContainsI(addressLine3))
                                addresses.Add(addressLine3);

                            organisation.Address1 = addresses.Count > 0 ? addresses[0] : null;
                            organisation.Address2 = addresses.Count > 1 ? addresses[1] : null;
                            organisation.Address3 = addresses.Count > 2 ? addresses[2] : null;

                            organisation.City = city;
                            organisation.County = county;
                            organisation.Country = country;
                            organisation.PostCode = postalCode;
                            organisation.PoBox = poBox;
                            organisation.AddressSource = "CoHo";
                        }

                        organisationsPage.Results.Add(organisation);
                    }
            }

            return organisationsPage;
        }

        private async Task<string> GetCompaniesAsync(string companyName, int page, int pageSize = 10,
            string httpException = null)
        {
            if (!string.IsNullOrWhiteSpace(httpException)) throw new HttpRequestException(httpException);

            var startIndex = page * pageSize - pageSize;

            var json = await _httpClient.GetStringAsync(
                $"/search/companies/?q={companyName}&items_per_page={pageSize}&start_index={startIndex}").ConfigureAwait(false);
            return json;
        }

        public static void SetupHttpClient(HttpClient httpClient, string apiServer)
        {
            httpClient.BaseAddress = new Uri(apiServer);

            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.ConnectionClose = false;
            ServicePointManager.FindServicePoint(httpClient.BaseAddress).ConnectionLeaseTimeout = 60 * 1000;
        }

        public static IAsyncPolicy<HttpResponseMessage> GetLinearRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(10,retryAttempt => TimeSpan.FromMilliseconds(new Random().Next(100, 1000)));
        }
        public static IAsyncPolicy<HttpResponseMessage> GetExponentialRetryPolicy()
        {
            var jitterer = new Random();
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(10, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) + TimeSpan.FromMilliseconds(jitterer.Next(0, 1000)));
        }
    }
}