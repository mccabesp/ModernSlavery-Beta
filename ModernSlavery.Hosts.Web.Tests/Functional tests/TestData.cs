using ModernSlavery.Core.Entities;
using ModernSlavery.Testing.Helpers.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModernSlavery.Hosts.Web.Tests
{
    public static class TestData
    {
        // This org should not come from DB
        // either creates or picks dynamically
        public static Organisation Organisation { get; set; }
            = OrganisationHelper.ListOrganisations(TestRunSetup.TestWebHost)
                .OrderBy(o => o.OrganisationName)
                .FirstOrDefault();

        public static string EmployerReference => Organisation.OrganisationReference;
        public static string OrgName => Organisation.OrganisationName;
        public static string RegisteredAddress => Organisation.GetAddressString(DateTime.Now);
        public static string SicCodes => Organisation.GetSicCodeIdsString(DateTime.Now);


        //multiple org details for Group Submission
        public static Organisation[] Organisations { get; } = OrganisationHelper.ListOrganisations(TestRunSetup.TestWebHost).OrderBy(o => o.OrganisationName).Take(10).ToArray();

        //scope url
        public const string ScopeUrl = "/scope/out";


        //todo clarify dummy data
        public const string InvalidEmployerReference = "invalid";
        public const string InvalidSecurityCode = "invalid";
        public const string ValidSecurityCode = "Valid";
        public const string ValidEmployerReference = "MZC3TMGQ";
        public const string ExpiredSecurityCode = "ABCD1234";
        public const string ExpiredEmployerReference = "A19XA11H";
             
       


        public const string OrgName_CantFind = "QWERTYUIOPQWERTYUIOP";
        public const string CompanyNumber_CantFind = "";
        public const string CharityNumber_CantFind = "";
        public const string MutualPartnerShipNumber_CantFind = "";
        public const string DUNS_CantFind = "";


    }
}
