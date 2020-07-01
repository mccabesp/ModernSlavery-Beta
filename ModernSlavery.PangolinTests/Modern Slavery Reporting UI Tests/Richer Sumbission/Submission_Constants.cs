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


        public static readonly string[] GoodsAndServices = new string[] { "Goods for resale", "Goods not for resale", "Services for sale", "Services used for operational purposes" };

        public static readonly string[] VulnerableGroups = new string[] { "Migrants", "Women", "Refugees", "Children", "Other vulnerable groups" };

        public static readonly string[] TypesOfWork = new string[] { "Temporary work", "Seasonal work", "Low skill or unskilled work", "Hazardous work", "Other type of work" };



        public static readonly string[] Financials = new string[] { "Under £36 million", "£36 million - £60 million", "£60 million - £100 million", "£100 million - £500 million", "£500 million+" };
        

        public static readonly string[] SupplyChainSectors = new string[] { "Domestic work", "Construction", "Manufacturing", "Agriculture, forestries and fishing", "Accommodation and food service activities", "Wholesale and trade", "Personal services", "Mining and quarrying", "Other sector" };

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


        public static readonly string[] African_Countries = new string[] { "Algeria, People's Democratic Republic of", "Angola, Republic of", "Botswana, Republic of", "British Indian Ocean Territory", "Burundi, Republic of", "Cameroon, Republic of", "Cape Verde, Republic of", "Central African Republic", "Chad, Republic of", "Comoros, Union of the", "Mayotte, Department of", "Congo, Republic of the", "Congo, Democratic Republic of the", "Benin, Republic of", "Equatorial Guinea, Republic of", "Ethiopia, Federal Democratic Republic of", "Eritrea, State of", "French Southern Territories", "Djibouti, Republic of", "Gabon, Gabonese Republic", "Gambia, Republic of the", "Ghana, Republic of", "Guinea, Republic of", "Côte d'Ivoire, Republic of", "Kenya, Republic of", "Lesotho, Kingdom of", "Liberia, Republic of", "Libya, State of", "Madagascar, Republic of", "Malawi, Republic of", "Mali, Republic of", "Mauritania, Islamic Republic of", "Mauritius, Republic of", "Morocco, Kingdom of", "Mozambique, Republic of", "Namibia, Republic of", "Niger, Republic of", "Nigeria, Federal Republic of", "Guinea - Bissau, Republic of", "Reunion", "Rwanda, Republic of", "Saint Helena", "São Tomé and Príncipe, Democratic Republic of", "Senegal, Republic of", "Seychelles, Republic of", "Sierra Leone, Republic of", "Somalia, Federal Republic of", "South Africa, Republic of", "Zimbabwe, Republic of", "South Sudan, Republic of", "Sudan, Republic of the", "Sahrawi Arab Democratic Republic", "Eswatini, Kingdom of", "Togo, Togolese Republic", "Tunisia, Republic of", "Uganda, Republic of", "Egypt, Arab Republic of", "Tanzania, United Republic of", "Burkina Faso", "Zambia, Republic of" };
        public static readonly string[] Asian_Countries = new string[] { "Afghanistan, Islamic Republic of", "Azerbaijan, Republic of", "Bahrain, Kingdom of", "Bangladesh, People's Republic of", "Armenia, Republic of", "Bhutan, Kingdom of", "Brunei Darussalam", "Myanmar, Union of", "Cambodia, Kingdom of", "Sri Lanka, Democratic Socialist Republic of", "China, People's Republic of", "Taiwan", "Christmas Island", "Cocos (Keeling) Islands", "Cyprus, Republic of", "Georgia", "Hong Kong, Special Administrative Region of China", "India, Republic of", "Indonesia, Republic of", "Iran, Islamic Republic of", "Iraq, Republic of", "Israel, State of", "Japan", "Kazakhstan, Republic of", "Jordan, Hashemite Kingdom of", "Korea, Democratic People's Republic of", "Korea, Republic of", "Kuwait, State of", "Kyrgyz Republic", "Lao People's Democratic Republic", "Lebanon, Lebanese Republic", "Macao, Special Administrative Region of China", "Malaysia", "Maldives, Republic of", "Mongolia", "Oman, Sultanate of", "Nepal, State of", "Pakistan, Islamic Republic of", "Palestine, state of", "Philippines, Republic of the", "Timor-Leste, Democratic Republic of", "Qatar, State of", "Russian Federation", "Saudi Arabia,", "Kingdom of", "Singapore, Republic of", "Vietnam, Socialist Republic of", "Syrian Arab Republic", "Tajikistan, Republic of", "Thailand, Kingdom of", "United Arab Emirates", "Turkey, Republic of", "Turkmenistan", "Egypt, Arab Republic of", "Uzbekistan, Republic of", "Yemen", "United Nations Neutral Zone", "Iraq-Saudi Arabia Neutral Zone", "Spratly Islands" };
        public static readonly string[] European_Countries = new string[] { "Albania, Republic of", "Andorra, Principality of", "Azerbaijan, Republic of", "Austria, Republic of", "Armenia, Republic of", "Belgium, Kingdom of", "Bosnia and Herzegovina", "Bulgaria, Republic of", "Belarus, Republic of", "Croatia, Republic of", "Cyprus, Republic of", "Czech Republic", "Denmark, Kingdom of", "Estonia, Republic of", "Faroe Islands", "Finland, Republic of", "Åland Islands", "France, French Republic", "Georgia", "Germany, Federal", "Republic of", "Gibraltar", "Greece, Hellenic Republic", "Holy See (Vatican City State)", "Hungary, Republic of", "Iceland, Republic of", "Ireland", "Italy, Italian Republic", "Kazakhstan, Republic of", "Kosovo, Republic of", "Latvia, Republic of", "Liechtenstein, Principality of", "Lithuania, Republic of", "Luxembourg, Grand Duchy of", "Malta, Republic of", "Monaco, Principality of", "Moldova, Republic of", "Montenegro", "Netherlands, Kingdom of the", "Norway, Kingdom of", "Poland, Republic of", "Portugal, Portuguese Republic", "Romania", "Russian Federation", "San Marino, Republic of", "Serbia, Republic of", "Slovakia (Slovak Republic)", "Slovenia, Republic of", "Spain,", "Kingdom of", "Svalbard & Jan Mayen Islands", "Sweden, Kingdom of", "Switzerland, Swiss Confederation", "Turkey, Republic of", "Ukraine", "North Macedonia, Republic of", "United Kingdom of Great Britain & Northern Ireland", "Guernsey, Bailiwick of", "Jersey, Bailiwick of", "Isle of Man" };
        public static readonly string[] NorthAmerican_Countries = new string[] { "Antigua and Barbuda", "Bahamas, Commonwealth of the", "Barbados", "Bermuda", "Belize", "British Virgin Islands", "Canada", "Cayman Islands", "Costa Rica, Republic of", "Cuba, Republic of", "Dominica, Commonwealth of", "Dominican", "Republic", "El Salvador, Republic of", "Greenland", "Grenada", "Guadeloupe", "Guatemala, Republic of", "Haiti, Republic of", "Honduras, Republic of", "Jamaica", "Martinique", "Mexico, United Mexican States", "Montserrat", "Netherlands Antilles", "Curaçao", "Aruba", "Sint Maarten (Netherlands)", "Bonaire, Sint Eustatius and Saba", "Nicaragua, Republic of", "United States Minor Outlying Islands", "Panama, Republic of", "Puerto Rico,", "Commonwealth of", "Saint Barthelemy", "Saint Kitts and Nevis, Federation of", "Anguilla", "Saint Lucia", "Saint Martin", "Saint Pierre and Miquelon", "Saint Vincent and the Grenadines", "Trinidad and Tobago, Republic of", "Turks and Caicos Islands", "United States of America", "United States Virgin Islands" };
        public static readonly string[] SouthAmerican_Countries = new string[] { "Argentina, Argentine Republic", "Bolivia, Republic of", "Brazil, Federative Republic of", "Chile, Republic of", "Colombia, Republic of", "Ecuador, Republic of", "Falkland Islands (Malvinas)", "French Guiana", "Guyana,", "Co-operative Republic of", "Paraguay, Republic of", "Peru, Republic of", "Suriname, Republic of", "Uruguay, Eastern Republic of", "Venezuela, Bolivarian Republic of" };
        public static readonly string[] Oceanic_Countries = new string[] { "American Samoa", "Australia, Commonwealth of", "Solomon Islands", "Cook Islands", "Fiji, Republic of", "French Polynesia", "Kiribati, Republic of", "Guam", "Nauru, Republic of", "New Caledonia", "Vanuatu, Republic of", "New Zealand", "Niue", "Norfolk Island", "Northern Mariana Islands, Commonwealth of the", "United States Minor Outlying Islands", "Micronesia, Federated States of", "Marshall Islands, Republic of the", "Palau, Republic of", "Papua New", "Guinea, Independent State of", "Pitcairn Islands", "Tokelau", "Tonga, Kingdom of", "Tuvalu", "Wallis and Futuna", "Samoa, Independent State of", "Disputed Territory" };
        public static readonly string[] Antarctic_Countries = new string[] { "Antarctica (the territory South of 60 deg S)", "Bouvet Island (Bouvetoya)", "South Georgia and the South Sandwich Islands", "Heard Island and McDonald Islands" };

    }
}
