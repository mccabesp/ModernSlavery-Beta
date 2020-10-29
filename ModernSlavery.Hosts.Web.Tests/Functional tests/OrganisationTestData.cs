using ModernSlavery.Core.Entities;
using ModernSlavery.Testing.Helpers.Classes;
using ModernSlavery.Testing.Helpers.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModernSlavery.Hosts.Web.Tests
{
    public class OrganisationTestData
    {
        private readonly BaseUITest _uiTest;
        public OrganisationTestData(BaseUITest uiTest)
        {
            _uiTest = uiTest;

            //Organisation=_uiTest.ListOrganisations().OrderBy(o => Guid.NewGuid()).Take(10).ToArray(); --This one orders randomly
            Organisations = _uiTest.ListOrganisations().OrderBy(o => o.OrganisationName).Take(10).ToArray();

            //Organisation=_uiTest.ListOrganisations().OrderBy(o => Guid.NewGuid()).FirstOrDefault(); --This one picks a random organisation
            Organisation = _uiTest.ListOrganisations().OrderBy(o => o.OrganisationName).FirstOrDefault();
        }

        // This org should not come from DB
        // either creates or picks dynamically
        public Organisation Organisation { get; set; }

        public string EmployerReference => Organisation.OrganisationReference;
        public string OrgName => Organisation.OrganisationName;
        public string RegisteredAddress => Organisation.GetAddressString(DateTime.Now);
        public string SicCodes => Organisation.GetSicCodeIdsString(DateTime.Now);


        //multiple org details for Group Submission
        public Organisation[] Organisations { get; }

        //scope url
        public string ScopeUrl = "/scope/out";


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
