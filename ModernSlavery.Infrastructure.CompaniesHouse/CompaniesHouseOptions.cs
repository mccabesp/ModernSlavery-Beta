using ModernSlavery.Core.Attributes;
using ModernSlavery.Core.Options;

namespace ModernSlavery.Infrastructure.CompaniesHouse
{
    [Options("CompaniesHouse")]
    public class CompaniesHouseOptions : IOptions
    {
        public string ApiKey { get; set; }
        public string ApiServer { get; set; }
        public string CompanyNumberRegexError { get; set; } = "Company number must contain 8 characters only";
        public int MaxRecords { get; set; } = 400;
    }
}