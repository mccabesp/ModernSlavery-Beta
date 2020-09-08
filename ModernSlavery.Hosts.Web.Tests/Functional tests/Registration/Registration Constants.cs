﻿using ModernSlavery.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModernSlavery.Testing.Helpers.Extensions;

namespace ModernSlavery.Hosts.Web.Tests
{
    public static class Registration
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

        //private static string _EmployerReference_Success;
        private static string _OrgName_InterFloor;
        ////private static string RegisteredAddress_InterFloor = "Broadway, Haslingden, Rossendale, Lancashire, BB4 4LS";
        //private static string Address1_InterFloor;
        //private static string Address2_InterFloor;
        //private static string Address3_InterFloor;
        //private static string PostCode_InterFloor;
        //private static string SicCodes_InterFloor;
        public static Organisation Organisation { get; } = OrganisationHelper.ListOrganisations(TestRunSetup.TestWebHost).OrderBy(o => o.OrganisationName).FirstOrDefault();

        public static string EmployerReference_Success => Organisation.OrganisationReference;
        public static string OrgName_InterFloor => Organisation.OrganisationName;
        public static string RegisteredAddress_InterFloor => Organisation.GetAddressString(DateTime.Now);
        public static string SicCodes_InterFloor => Organisation.GetSicCodeIdsString(DateTime.Now);


        public const string OrgName_CantFind = "QWERTYUIOPQWERTYUIOP";
        public const string CompanyNumber_CantFind = "";
        public const string CharityNumber_CantFind = "";
        public const string MutualPartnerShipNumber_CantFind = "";
        public const string DUNS_CantFind = "";




        public const string EmployerReference_NationalHeritage = "MZMGZPMM";
        public const string SecurtiyCode_NationalHeritage = "ABCD1234";



    }
}
