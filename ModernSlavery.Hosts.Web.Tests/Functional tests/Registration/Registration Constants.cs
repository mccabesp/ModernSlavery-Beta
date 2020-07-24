using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModernSlavery.Hosts.Web.Tests
{
    class Registration
    {
        //todo clarify dummy data
        public const string InvalidEmployerReference = "invalid";
        public const string InvalidSecurityCode = "invalid";
        public const string ValidSecurityCode = "Valid";
        public const string ValidEmployerReference = "MZC3TMGQ";
        public const string ExpiredSecurityCode = "ABCD1234";
        public const string ExpiredEmployerReference = "A19XA11H";

        public const string EmployerReference_Milbrook = "31QPYPBB";
        public const string OrgName_Millbrook = "MILLBROOK HEALTHCARE LTD";
        public const string CompanyNumber_Millbrook = "00833987";
        public const string RegisteredAddress_Millbrook = "Nutsey Lane Calmore Ind Estate, Totton, Southampton, Hampshire, United Kingdom, SO40 3XJ";
        public const string SecurtiyCode_Millbrook = "ABCD1234";
        public static readonly Tuple<string, string, string> SicCode_Milbrook = new Tuple<string, string, string>("Human health and social work activities", "86900", "Other human health activities");

        public const string EmployerReference_Blackpool = "NCSQN8T6";
        public const string OrgName_Blackpool = "Blackpool Council";
        public const string RegisteredAddress_Blackpool = "PO Box 4, Blackpool, FY1 1NA";
        public const string Address1_Blackpool = "PO Box 4";
        public const string Address2_Blackpool = "";
        public const string Address3_Blackpool = "Blackpool";
        public const string PostCode_Blackpool = "FY1 1NA";
        //codes 1, 84110
        public const string SicCodes_Blackpool = "Public Sector, General public administration activities";

        public const string EmployerReference_InterFloor = "B72GL8R2";
        public const string OrgName_InterFloor = "INTERFLOOR LIMITED";
        public const string RegisteredAddress_InterFloor = "Broadway, Haslingden, Rossendale, Lancashire, BB4 4LS";
        public const string Address1_InterFloor = "Broadway, Haslingden";
        public const string Address2_InterFloor = "";
        public const string Address3_InterFloor = "Rossendale";
        public const string PostCode_InterFloor = "BB4 4LS";
        //codes 1, 84110
        public const string SicCodes_InterFloor = "Public Sector, General public administration activities";


        public const string OrgName_CantFind = "QWERTYUIOPQWERTYUIOP";
        public const string CompanyNumber_CantFind = "";
        public const string CharityNumber_CantFind = "";
        public const string MutualPartnerShipNumber_CantFind = "";
        public const string DUNS_CantFind = "";




        public const string EmployerReference_NationalHeritage = "MZMGZPMM";
        public const string SecurtiyCode_NationalHeritage = "ABCD1234";



    }
}
