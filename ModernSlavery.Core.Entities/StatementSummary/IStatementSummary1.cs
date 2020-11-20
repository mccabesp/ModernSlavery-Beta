using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace ModernSlavery.Core.Entities.StatementSummary
{
    public interface IStatementSummary1
    {
        #region Policies Fields
        public enum PolicyTypes
        {
            Unknown = 0,
            [Description("Freedom of workers to terminate employment")] FreedomToTerminate,
            [Description("Freedom of movement")] FreedomOfMovement,
            [Description("Freedom of association")] FreedomOfAssociation,
            [Description("Prohibits any threat of violence, harassment and intimidation")] ViolenceHarassmentIntimidation,
            [Description("Prohibits the use of worker-paid recruitment fees")] RecruitmentFees,
            [Description("Prohibits compulsory overtime")] CompulsoryOvertime,
            [Description("Prohibits child labour")] ChildLabour,
            [Description("Prohibits discrimination")] Discrimination,
            [Description("Prohibits confiscation of workers original identification documents")] DocumentRetention,
            [Description("Provides access to remedy, compensation and justice for victims of modern slavery")] RemedyAccess,
            [Description("Other")] Other,
            [Description("Your organisation's policies do not include any of these provisions in relation to modern slavery")] None
        }

        SortedSet<PolicyTypes> Policies { get; set; }

        string OtherPolicies { get; set; }
        #endregion

        #region Training Fields
        public enum TrainingTargetTypes
        {
            Unknown,
            [Description("Your whole organisation")] OwnOrganisation,
            [Description("Your front line staff")] OwnStaff,
            [Description("Human resources")] OwnHumanResources,
            [Description("Executive-level staff")] OwnExecutives,
            [Description("Procurement staff")] OwnProcurers,
            [Description("Your suppliers")] Suppliers,
            [Description("The wider community")] WiderCommunity,
            [Description("Other")] Other,
            [Description("Your organisation did not provide training on modern slavery during the period of the statement")] None
        }

        SortedSet<TrainingTargetTypes> TrainingTargets { get; set; }

        string OtherTrainingTargets { get; set; }

        #endregion

        #region Partner Fields
        public enum PartnerTypes
        {
            Unknown,
            [Description("Your suppliers")] OwnSuppliers,
            [Description("Trade unions or worker representative groups")] TradeUnions,
            [Description("Civil society organisations")] CivilSocietyOrganisations,
            [Description("Professional auditors")] ProfessionalAuditors,
            [Description("Workers within your organisation")] OwnWorkers,
            [Description("Workers within your supply chain")] SupplierWorkers,
            [Description("Central or local government")] Government,
            [Description("Law enforcement, such as police, GLAA and other local labour market inspectorates")] LawEnforcement,
            [Description("Businesses in your industry or sector")] Businesses,
            [Description("Your organisation did not engage with others to help monitor working conditions across your organisation and supply chain")] None,
        }

        SortedSet<PartnerTypes> Partners { get; set; }

        string OtherPartners { get; set; }
        #endregion

        #region Social Audit Fields
        public enum SocialAuditTypes
        {
            Unknown = 0,
            [Description("Audit conducted by your staff")] OwnStaff,
            [Description("Third party audit arranged by your organisation")] ThirdPartyOrganisation,
            [Description("Audit conducted by your supplier’s staff")] SupplierStaff,
            [Description("Third party audit arranged by your supplier")] ThirdPartySupplier,
            [Description("Announced audits")] Announced,
            [Description("Unannounced audits")] Unannounced,
            [Description("Your organisation did not carry out any social audits during the period of the statement")] None,
        }

        SortedSet<SocialAuditTypes> SocialAudits { get; set; }

        string OtherSocialAudits { get; set; }
        #endregion

        #region Grievance Mechanism Fields
        public enum GrievanceMechanismTypes
        {
            Unknown,
            [Description("Whistleblowing services")] WhistleblowingServices,
            [Description("Worker voice platforms")] WorkerVoicePlatforms,
            [Description("Your organisation did not have anonymous grievance mechanisms in place during the period of the statement")] None,
            [Description("Other grievance mechanism")] Other,
        }

        SortedSet<GrievanceMechanismTypes> GrievanceMechanisms { get; set; }

        string OtherGrievanceMechanisms { get; set; }
        #endregion

        #region Other Work Conditions Monitoring Fields
        string OtherWorkConditionsMonitoring { get; set; }
        #endregion

        #region Risks
        public class StatementRisk
        {
            public string Description { get; set; }

            #region Risk Source Fields
            public enum RiskSourceTypes
            {
                Unknown,
                [Description("Within your own operations")] OwnOperations,
                [Description("Within your supply chains")] SupplyChains,
                [Description("Other")] Other,
            }

            public RiskSourceTypes LikelySource { get; set; }

            public enum SupplyChainTierTypes
            {
                Unknown,
                [Description("Tier 1 suppliers")] Tier1,
                [Description("Tier 2 suppliers")] Tier2,
                [Description("Tier 3 suppliers and below")] Tier3,
                [Description("Don't know")] None,
            }

            public List<SupplyChainTierTypes> SupplyChainTiers { get; set; } = new List<SupplyChainTierTypes>();

            public string OtherLikelySource { get; set; }
            #endregion

            #region Risk Target Fields
            public enum RiskTargetTypes
            {
                Unknown,
                [Description("Women")] Women,
                [Description("Migrants")] Migrants,
                [Description("Refugees")] Refugees,
                [Description("Children")] Children,
                [Description("Other vulnerable group(s)")] Other,
            }

            public SortedSet<RiskTargetTypes> Targets { get; set; } = new SortedSet<RiskTargetTypes>();

            public string OtherTargets { get; set; }
            #endregion

            #region Actions or Plans Field
            public string ActionsOrPlans { get; set; }
            #endregion

            #region Risk Location Fields
            /// <summary>
            /// ISO 3166-1 Alpha 2 Country Codes (source: https://gist.github.com/jplwood/4f77b55cfedf2820dce0dfcd3ee0c3ea)
            /// </summary>
            public enum CountryTypes
            {
                Unknown = 0,
                [Description("Afghanistan")] AF = 1,
                [Description("Åland Islands")] AX = 2,
                [Description("Albania")] AL = 3,
                [Description("Algeria")] DZ = 4,
                [Description("American Samoa")] AS = 5,
                [Description("Andorra")] AD = 6,
                [Description("Angola")] AO = 7,
                [Description("Anguilla")] AI = 8,
                [Description("Antarctica")] AQ = 9,
                [Description("Antigua and Barbuda")] AG = 10,
                [Description("Argentina")] AR = 11,
                [Description("Armenia")] AM = 12,
                [Description("Aruba")] AW = 13,
                [Description("Australia")] AU = 14,
                [Description("Austria")] AT = 15,
                [Description("Azerbaijan")] AZ = 16,
                [Description("Bahamas")] BS = 17,
                [Description("Bahrain")] BH = 18,
                [Description("Bangladesh")] BD = 19,
                [Description("Barbados")] BB = 20,
                [Description("Belarus")] BY = 21,
                [Description("Belgium")] BE = 22,
                [Description("Belize")] BZ = 23,
                [Description("Benin")] BJ = 24,
                [Description("Bermuda")] BM = 25,
                [Description("Bhutan")] BT = 26,
                [Description("Bolivia (Plurinational State of)")] BO = 27,
                [Description("Bonaire, Sint Eustatius and Saba")] BQ = 28,
                [Description("Bosnia and Herzegovina")] BA = 29,
                [Description("Botswana")] BW = 30,
                [Description("Bouvet Island")] BV = 31,
                [Description("Brazil")] BR = 32,
                [Description("British Indian Ocean Territory")] IO = 33,
                [Description("Brunei Darussalam")] BN = 34,
                [Description("Bulgaria")] BG = 35,
                [Description("Burkina Faso")] BF = 36,
                [Description("Burundi")] BI = 37,
                [Description("Cabo Verde")] CV = 38,
                [Description("Cambodia")] KH = 39,
                [Description("Cameroon")] CM = 40,
                [Description("Canada")] CA = 41,
                [Description("Cayman Islands")] KY = 42,
                [Description("Central African Republic")] CF = 43,
                [Description("Chad")] TD = 44,
                [Description("Chile")] CL = 45,
                [Description("China")] CN = 46,
                [Description("Christmas Island")] CX = 47,
                [Description("Cocos (Keeling) Islands")] CC = 48,
                [Description("Colombia")] CO = 49,
                [Description("Comoros")] KM = 50,
                [Description("Congo")] CG = 51,
                [Description("Congo (Democratic Republic of the)")] CD = 52,
                [Description("Cook Islands")] CK = 53,
                [Description("Costa Rica")] CR = 54,
                [Description("Côte d'Ivoire")] CI = 55,
                [Description("Croatia")] HR = 56,
                [Description("Cuba")] CU = 57,
                [Description("Curaçao")] CW = 58,
                [Description("Cyprus")] CY = 59,
                [Description("Czechia")] CZ = 60,
                [Description("Denmark")] DK = 61,
                [Description("Djibouti")] DJ = 62,
                [Description("Dominica")] DM = 63,
                [Description("Dominican Republic")] DO = 64,
                [Description("Ecuador")] EC = 65,
                [Description("Egypt")] EG = 66,
                [Description("El Salvador")] SV = 67,
                [Description("Equatorial Guinea")] GQ = 68,
                [Description("Eritrea")] ER = 69,
                [Description("Estonia")] EE = 70,
                [Description("Ethiopia")] ET = 71,
                [Description("Falkland Islands (Malvinas)")] FK = 72,
                [Description("Faroe Islands")] FO = 73,
                [Description("Fiji")] FJ = 74,
                [Description("Finland")] FI = 75,
                [Description("France")] FR = 76,
                [Description("French Guiana")] GF = 77,
                [Description("French Polynesia")] PF = 78,
                [Description("French Southern Territories")] TF = 79,
                [Description("Gabon")] GA = 80,
                [Description("Gambia")] GM = 81,
                [Description("Georgia")] GE = 82,
                [Description("Germany")] DE = 83,
                [Description("Ghana")] GH = 84,
                [Description("Gibraltar")] GI = 85,
                [Description("Greece")] GR = 86,
                [Description("Greenland")] GL = 87,
                [Description("Grenada")] GD = 88,
                [Description("Guadeloupe")] GP = 89,
                [Description("Guam")] GU = 90,
                [Description("Guatemala")] GT = 91,
                [Description("Guernsey")] GG = 92,
                [Description("Guinea")] GN = 93,
                [Description("Guinea-Bissau")] GW = 94,
                [Description("Guyana")] GY = 95,
                [Description("Haiti")] HT = 96,
                [Description("Heard Island and McDonald Islands")] HM = 97,
                [Description("Holy See")] VA = 98,
                [Description("Honduras")] HN = 99,
                [Description("Hong Kong")] HK = 100,
                [Description("Hungary")] HU = 101,
                [Description("Iceland")] IS = 102,
                [Description("India")] IN = 103,
                [Description("Indonesia")] ID = 104,
                [Description("Iran (Islamic Republic of)")] IR = 105,
                [Description("Iraq")] IQ = 106,
                [Description("Ireland")] IE = 107,
                [Description("Isle of Man")] IM = 108,
                [Description("Israel")] IL = 109,
                [Description("Italy")] IT = 110,
                [Description("Jamaica")] JM = 111,
                [Description("Japan")] JP = 112,
                [Description("Jersey")] JE = 113,
                [Description("Jordan")] JO = 114,
                [Description("Kazakhstan")] KZ = 115,
                [Description("Kenya")] KE = 116,
                [Description("Kiribati")] KI = 117,
                [Description("Korea (Democratic People's Republic of)")] KP = 118,
                [Description("Korea (Republic of)")] KR = 119,
                [Description("Kuwait")] KW = 120,
                [Description("Kyrgyzstan")] KG = 121,
                [Description("Lao People's Democratic Republic")] LA = 122,
                [Description("Latvia")] LV = 123,
                [Description("Lebanon")] LB = 124,
                [Description("Lesotho")] LS = 125,
                [Description("Liberia")] LR = 126,
                [Description("Libya")] LY = 127,
                [Description("Liechtenstein")] LI = 128,
                [Description("Lithuania")] LT = 129,
                [Description("Luxembourg")] LU = 130,
                [Description("Macao")] MO = 131,
                [Description("Macedonia (the former Yugoslav Republic of)")] MK = 132,
                [Description("Madagascar")] MG = 133,
                [Description("Malawi")] MW = 134,
                [Description("Malaysia")] MY = 135,
                [Description("Maldives")] MV = 136,
                [Description("Mali")] ML = 137,
                [Description("Malta")] MT = 138,
                [Description("Marshall Islands")] MH = 139,
                [Description("Martinique")] MQ = 140,
                [Description("Mauritania")] MR = 141,
                [Description("Mauritius")] MU = 142,
                [Description("Mayotte")] YT = 143,
                [Description("Mexico")] MX = 144,
                [Description("Micronesia (Federated States of)")] FM = 145,
                [Description("Moldova (Republic of)")] MD = 146,
                [Description("Monaco")] MC = 147,
                [Description("Mongolia")] MN = 148,
                [Description("Montenegro")] ME = 149,
                [Description("Montserrat")] MS = 150,
                [Description("Morocco")] MA = 151,
                [Description("Mozambique")] MZ = 152,
                [Description("Myanmar")] MM = 153,
                [Description("Namibia")] NA = 154,
                [Description("Nauru")] NR = 155,
                [Description("Nepal")] NP = 156,
                [Description("Netherlands")] NL = 157,
                [Description("New Caledonia")] NC = 158,
                [Description("New Zealand")] NZ = 159,
                [Description("Nicaragua")] NI = 160,
                [Description("Niger")] NE = 161,
                [Description("Nigeria")] NG = 162,
                [Description("Niue")] NU = 163,
                [Description("Norfolk Island")] NF = 164,
                [Description("Northern Mariana Islands")] MP = 165,
                [Description("Norway")] NO = 166,
                [Description("Oman")] OM = 167,
                [Description("Pakistan")] PK = 168,
                [Description("Palau")] PW = 169,
                [Description("Palestine, State of")] PS = 170,
                [Description("Panama")] PA = 171,
                [Description("Papua New Guinea")] PG = 172,
                [Description("Paraguay")] PY = 173,
                [Description("Peru")] PE = 174,
                [Description("Philippines")] PH = 175,
                [Description("Pitcairn")] PN = 176,
                [Description("Poland")] PL = 177,
                [Description("Portugal")] PT = 178,
                [Description("Puerto Rico")] PR = 179,
                [Description("Qatar")] QA = 180,
                [Description("Réunion")] RE = 181,
                [Description("Romania")] RO = 182,
                [Description("Russian Federation")] RU = 183,
                [Description("Rwanda")] RW = 184,
                [Description("Saint Barthélemy")] BL = 185,
                [Description("Saint Helena, Ascension and Tristan da Cunha")] SH = 186,
                [Description("Saint Kitts and Nevis")] KN = 187,
                [Description("Saint Lucia")] LC = 188,
                [Description("Saint Martin (French part)")] MF = 189,
                [Description("Saint Pierre and Miquelon")] PM = 190,
                [Description("Saint Vincent and the Grenadines")] VC = 191,
                [Description("Samoa")] WS = 192,
                [Description("San Marino")] SM = 193,
                [Description("Sao Tome and Principe")] ST = 194,
                [Description("Saudi Arabia")] SA = 195,
                [Description("Senegal")] SN = 196,
                [Description("Serbia")] RS = 197,
                [Description("Seychelles")] SC = 198,
                [Description("Sierra Leone")] SL = 199,
                [Description("Singapore")] SG = 200,
                [Description("Sint Maarten (Dutch part)")] SX = 201,
                [Description("Slovakia")] SK = 202,
                [Description("Slovenia")] SI = 203,
                [Description("Solomon Islands")] SB = 204,
                [Description("Somalia")] SO = 205,
                [Description("South Africa")] ZA = 206,
                [Description("South Georgia and the South Sandwich Islands")] GS = 207,
                [Description("South Sudan")] SS = 208,
                [Description("Spain")] ES = 209,
                [Description("Sri Lanka")] LK = 210,
                [Description("Sudan")] SD = 211,
                [Description("Suriname")] SR = 212,
                [Description("Svalbard and Jan Mayen")] SJ = 213,
                [Description("Swaziland")] SZ = 214,
                [Description("Sweden")] SE = 215,
                [Description("Switzerland")] CH = 216,
                [Description("Syrian Arab Republic")] SY = 217,
                [Description("Taiwan, Province of China[a]")] TW = 218,
                [Description("Tajikistan")] TJ = 219,
                [Description("Tanzania, United Republic of")] TZ = 220,
                [Description("Thailand")] TH = 221,
                [Description("Timor-Leste")] TL = 222,
                [Description("Togo")] TG = 223,
                [Description("Tokelau")] TK = 224,
                [Description("Tonga")] TO = 225,
                [Description("Trinidad and Tobago")] TT = 226,
                [Description("Tunisia")] TN = 227,
                [Description("Turkey")] TR = 228,
                [Description("Turkmenistan")] TM = 229,
                [Description("Turks and Caicos Islands")] TC = 230,
                [Description("Tuvalu")] TV = 231,
                [Description("Uganda")] UG = 232,
                [Description("Ukraine")] UA = 233,
                [Description("United Arab Emirates")] AE = 234,
                [Description("United Kingdom of Great Britain and Northern Ireland")] GB = 235,
                [Description("United States of America")] US = 236,
                [Description("United States Minor Outlying Islands")] UM = 237,
                [Description("Uruguay")] UY = 238,
                [Description("Uzbekistan")] UZ = 239,
                [Description("Vanuatu")] VU = 240,
                [Description("Venezuela (Bolivarian Republic of)")] VE = 241,
                [Description("Viet Nam")] VN = 242,
                [Description("Virgin Islands (British)")] VG = 243,
                [Description("Virgin Islands (U.S.)")] VI = 244,
                [Description("Wallis and Futuna")] WF = 245,
                [Description("Western Sahara")] EH = 246,
                [Description("Yemen")] YE = 247,
                [Description("Zambia")] ZM = 248,
                [Description("Zimbabwe")] ZW = 249,
            }

            public SortedSet<CountryTypes> Countries { get; set; } = new SortedSet<CountryTypes>();
            #endregion

            #region Methods
            public bool IsEmpty(bool ignoreDescription = false)
            {
                return (ignoreDescription || string.IsNullOrWhiteSpace(Description))
                    && LikelySource == RiskSourceTypes.Unknown && string.IsNullOrWhiteSpace(OtherLikelySource)
                    && (Targets == null || Targets.Count == 0) && string.IsNullOrWhiteSpace(OtherTargets)
                    && (Countries == null || Countries.Count == 0);
            }
            #endregion
        }

        List<StatementRisk> Risks { get; set; }

        #endregion

        #region Forced Labour Fields
        public enum IndicatorTypes
        {
            Unknown,
            [Description("Abuse of vulnerability")] VulnerabilityAbuse,
            [Description("Deception")] Deception,
            [Description("Restriction of movement")] MovementRestriction,
            [Description("Isolation")] Isolation,
            [Description("Physical and sexual violence")] Violence,
            [Description("Intimidation and threats")] ThreatIntimidation,
            [Description("Retention of identity documents")] DocumentRetention,
            [Description("Withholding of wages")] WageWithholding,
            [Description("Debt bondage")] DebtBondage,
            [Description("Abusive working and living conditions")] WorkLiveConditions,
            [Description("Excessive overtime")] ExcessiveOvertime,
            [Description("Other")] Other,
            [Description("My statement does not refer to finding any ILO indicators of forced labour")] None,
        }

        SortedSet<IndicatorTypes> Indicators { get; set; }

        string OtherIndicators { get; set; }
        #endregion

        #region Remediation Fields
        public enum RemediationTypes
        {
            Unknown,
            [Description("Financial remediation, including repayment of recruitment fees")] FeeRepayment,
            [Description("Change in policy")] PolicyChange,
            [Description("Change in training")] TrainingChange,
            [Description("Referring victims to government service")] VictimReferral,
            [Description("Supporting victims via NGO")] NGOSupport,
            [Description("Supporting investigations by relevant authorities")] CriminalJustice,
            [Description("Other")] Other,
            [Description("My statement does not refer to actions taken in response to finding indicators of forced labour")] None,
        }

        SortedSet<RemediationTypes> Remediations { get; set; }

        string OtherRemediations { get; set; }
        #endregion

        #region Progress Measuring Fields
        string ProgressMeasures { get; set; }
        #endregion

    }
}
