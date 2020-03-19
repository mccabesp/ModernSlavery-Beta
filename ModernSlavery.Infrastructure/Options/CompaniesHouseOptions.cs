using System;
using System.Collections.Generic;
using System.Text;
using ModernSlavery.SharedKernel.Options;

namespace ModernSlavery.Infrastructure.Data
{
    [Options("CompaniesHouse")]
    public class CompaniesHouseOptions:IOptions
    {
        public string ApiKey;
        public string ApiServer;
        public int MaxRecords=400;
        public string CompanyNumberRegexError = "Company number must contain 8 characters only";
    }
}
