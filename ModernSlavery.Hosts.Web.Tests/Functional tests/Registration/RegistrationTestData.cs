using ModernSlavery.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModernSlavery.Testing.Helpers.Extensions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ModernSlavery.Testing.Helpers.Classes;

namespace ModernSlavery.Hosts.Web.Tests
{
    public class RegistrationTestData
    {
        private readonly BaseUITest _uiTest;
        public RegistrationTestData(BaseUITest uiTest)
        {
            _uiTest = uiTest;
            //Organisation=_uiTest.ListOrganisations().OrderBy(o => Guid.NewGuid()).FirstOrDefault(); --This one picks a random organisation
            Organisation=_uiTest.ListOrganisations().OrderBy(o => o.OrganisationName).FirstOrDefault();
        }

        //todo clarify dummy data
        public const string InvalidEmployerReference = "invalid";
        public const string ValidSecurityCode = "Valid";

        public const string SecurtiyCode_Millbrook = "ABCD1234";
        public readonly Tuple<string, string, string> SicCode_Milbrook = new Tuple<string, string, string>("Human health and social work activities", "86900", "Other human health activities");

        public const string EmployerReference_Blackpool = "NCSQN8T6";
        public const string OrgName_Blackpool = "Blackpool Council";
        public const string RegisteredAddress_Blackpool = "PO Box 4, Blackpool, FY1 1NA";
        public const string Address1_Blackpool = "PO Box 4";
        public const string Address2_Blackpool = "The Lane";
        public const string Address3_Blackpool = "Blackpool";
        public const string PostCode_Blackpool = "FY1 1NA";
        //codes 1, 84110
        public const string SicCodes_Blackpool = "Public Sector, General public administration activities";

        //private string _EmployerReference_Success;
        private string _OrgName_InterFloor;
        ////private string RegisteredAddress_InterFloor = "Broadway, Haslingden, Rossendale, Lancashire, BB4 4LS";
        //private string Address1_InterFloor;
        //private string Address2_InterFloor;
        //private string Address3_InterFloor;
        //private string PostCode_InterFloor;
        //private string SicCodes_InterFloor;
        public Organisation Organisation { get; set; }

        public const string OrgName_CantFind = "QWERTYUIOPQWERTYUIOP";
        public const string CompanyNumber_CantFind = "12345678";
        public const string CharityNumber_CantFind = "";
        public const string MutualPartnerShipNumber_CantFind = "";
        public const string DUNS_CantFind = "";

        public const string EmployerReference_NationalHeritage = "MZMGZPMM";
        public const string SecurtiyCode_NationalHeritage = "ABCD1234";
    }
}
