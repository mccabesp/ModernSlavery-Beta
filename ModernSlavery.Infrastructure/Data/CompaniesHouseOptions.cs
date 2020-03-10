using System;
using System.Collections.Generic;
using System.Text;

namespace ModernSlavery.Infrastructure.Data
{
    public class CompaniesHouseOptions
    {
        public string ApiKey;
        public string ApiServer;
        public int MaxRecords=400;
        public string CompanyNumberRegexError = "Company number must contain 8 characters only";
    }
}
