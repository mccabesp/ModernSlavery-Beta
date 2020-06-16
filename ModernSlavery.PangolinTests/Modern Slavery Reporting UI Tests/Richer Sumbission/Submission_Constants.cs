using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modern_Slavery_Reporting_UI_Tests
{
    class Submission
    {
        public const string EmployerReference_Milbrook = "31QPYPBB";
        public const string OrgName_Millbrook = "MILLBROOK HEALTHCARE LTD";
        public const string CompanyNumber_Millbrook = "00833987";
        public const string RegisteredAddress_Millbrook = "Nutsey Lane Calmore Ind Estate, Totton, Southampton, Hampshire, United Kingdom, SO40 3XJ";
        public const string SecurtiyCode_Millbrook = "ABCD1234";

        public const string EmployerReference_Blackpool = "NCSQN8T6";
        public const string OrgName_Blackpool = "Blackpool Council";
        public const string RegisteredAddress_Blackpool = "PO Box 4, Blackpool, FY1 1NA";
        public const string Address1_Blackpool = "PO Box 4";
        public const string Address2_Blackpool = "";
        public const string Address3_Blackpool = "Blackpool";
        public const string PostCode_Blackpool = "FY1 1NA";
        //codes 1, 84110
        public const string SicCodes_Blackpool = "Public Sector, General public administration activities";
        public const string Url_Blackpool = "www.test.com";
        public const string DateFrom_Blackpool = "01/08/2019";
        public const string DateTo_Blackpool = "31/07/2020";
        public const string DateApproved_Blackpool = "31/06/2020";
        public const string FirstName_Blackpool = "FY1 1NA";
        public const string LastName_Blackpool = "FY1 1NA";
        public const string JobTitle_Blackpool = "Director";


        public const string EmployerReference_InterFloor = "B72GL8R2";
        public const string OrgName_InterFloor = "InterFloor Limited";
        public const string RegisteredAddress_InterFloor = "Broadway, Haslingden, Rossendale, BB4 4LS";
        public const string Address1_InterFloor = "Broadway, Haslingden";
        public const string Address2_InterFloor = "";
        public const string Address3_InterFloor = "Rossendale";
        public const string PostCode_InterFloor = "BB4 4LS";
        //codes 1, 84110
        public const string SicCodes_InterFloor = "Public Sector, General public administration activities";

        public static readonly string[] Sectors = new string[] { "All", "Administrative and support service activities", "Your organisation", "Agriculture, Forestry and Fishing", "Arts,", "entertainment and recreation", "Construction", "Education", "Electricity, gas, steam and air conditioning supply", "Financial and insurance activities", "Human health and social work activities", "Information and communication", "Manufacturing", "Mining and Quarrying", "Other service activities", "Professional scientific and technical activities", "Public administration and defence", "Public sector", "Real estate activities", "Transportation and storage", "Water supply,", "sewerage, waste management and remediation activities", "Wholesale and retail trade" } ;

        public static readonly string[] Financials = new string[] { "Under £36 million", "£36 million - £60 million", "£60 million - £100 million", "£100 million - £500 million", "£500 million+" };

    }
}
