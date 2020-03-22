using ModernSlavery.Core.SharedKernel.Options;

namespace ModernSlavery.Infrastructure.CompaniesHouse
{
    [Options("CompaniesHouse")]
    public class CompaniesHouseOptions : IOptions
    {
        public string ApiKey;
        public string ApiServer;
        public string CompanyNumberRegexError = "Company number must contain 8 characters only";
        public int MaxRecords = 400;
    }
}