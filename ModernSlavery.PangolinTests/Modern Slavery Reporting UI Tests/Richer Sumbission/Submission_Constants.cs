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

        public const string YourMSStatement_URL = "https://bing.com";
        public const string YourMSStatement_Date_Day = "1";
        public const string YourMSStatement_Date_Month = "02";
        public const string YourMSStatement_Date_Year = "2019";
        public const string YourMSStatement_To_Day = "31";
        public const string YourMSStatement_To_Month = "01";
        public const string YourMSStatement_To_Year = "2020";
        public const string YourMSStatement_First = "Roger";
        public const string YourMSStatement_Last = "Reporter";
        public const string YourMSStatement_JobTitle = "Modern Slavery Officer";
        public const string YourMSStatement_ApprovalDate_Day = "27";
        public const string YourMSStatement_ApprovalDate_Month = "06";
        public const string YourMSStatement_ApprovalDate_Year = "2020";

        public static readonly string[] YourOrganisation_Sectors = new string[] { "Administrative and support service activities", "Your organisation", "Agriculture, Forestry and Fishing", "Arts,","Electricity, gas, steam and air conditioning supply", "Financial and insurance activities", "trade" };
        public const string YourOrganisation_Turnover = "£60 million - £100 million";

        public static readonly string[] Policies_SelectedPolicies = new string[] { "Freedom of association", "Prohibits discrimination", "Other" };
        public  const string Policies_OtherDetails = "We have many other policies in place";


        public static readonly string[] SupplyChainRisks_SelectedGoodsAndServices = new string[] { "Goods not for resale", "Services for sale"};

        public static readonly string[] SupplyChainRisks_SelectedVulnerableGroups = new string[] { "Migrants", "Children", "Other vulnerable groups" };
        public const string SupplyChainRisks_OtherVulernableGroupsDetails = "People who are at risk.";

        public static readonly string[] SupplyChainRisks_SelectedTypeOfWorks = new string[] { "Seasonal work", "Hazardous work", "Other type of work" };
        public const string SupplyChainRisks_OtherTypeOfWorkDetails = "Voluntary work";

        public static readonly string[] SupplyChainRisks_SelectedSectors = new string[] { "Manufacturing", "Personal services", "Mining and quarrying", "Other sector" };
        public const string SupplyChainRisks_OtherSectorDetails = "Voluntary sector";
        public const string SuppliChainRisks_OtherArea = "These are details about an additional area not mentioned above";

        public static readonly string[] SupplyChainRisks_SelectedCountriesAfrica = new string[] { "Benin, Republic of", "Reunion", "Burkina Faso" };
        public static readonly string[] SupplyChainRisks_SelectedCountriesAsia = new string[] { "Afghanistan, Islamic Republic of", "Japan", "Mongolia", "Turkmenistan", "United Nations Neutral Zone" };
        public static readonly string[] SupplyChainRisks_SelectedCountriesEurope = new string[] { "Georgia", "Reunion", "United Kingdom of Great Britain & Northern Ireland" };
        public static readonly string[] SupplyChainRisks_SelectedCountriesNorthAmerica = new string[] { "Anguilla", "Martinique", "Cuba, Republic of" };
        public static readonly string[] SupplyChainRisks_SelectedCountriesOceania = new string[] { "New Zealand"};
        public static readonly string[] SupplyChainRisks_SelectedCountriesSouthAmerica = new string[] { "French Guiana", "Peru, Republic of"};
        public static readonly string[] SupplyChainRisks_SelectedCountriesAntarctica= new string[] { "Antarctica (the territory South of 60 deg S)", "Bouvet Island (Bouvetoya)", "South Georgia and the South Sandwich Islands", "Heard Island and McDonald Islands" };

        public static readonly string[] SupplyChainRisks_SelectedPartnerships = new string[] { "with civil society organisations", "with central or local government", "with multi-stakeholder initiatives" };

        public static readonly string[] SupplyChainRisks_SelectedSocialAudits= new string[] { "by a third party", "other"};
        public const string SupplyChainRisks_OtherSocialAudits = "Another audit";

        public static readonly string[] SupplyChainRisks_SelectedGrievanceMechanisms = new string[] { "whistleblowing services", "worker voice technology"};

        public const string SupplyChainRisks_IndicatorDetails = "There have been signs.";

        public const string SupplyChainRisks_InStanceDetails = "A few here and there.";

        public static readonly string[] SupplyChainRisks_SelectedRemediationActions = new string[] { "repayment of recruitment fees", "other" };

        public static readonly string[] SelectedTrainings = new string[] { "C-Suite", "Whole organisation", "Suppliers", "other" };
        public const string OtherTrainings = "We had a group session.";

        public const string MonitoringProgress = "Keeping detailed notes";
        public const string MonitoringAchievements = "We didn't have any";




    }
}
