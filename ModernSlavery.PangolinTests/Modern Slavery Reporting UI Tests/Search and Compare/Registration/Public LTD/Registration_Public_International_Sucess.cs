﻿using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class Registration_Public_International_Sucess : UITest
    {
        [TestMethod]
        public override void RunTest()
        {
            Run<Create_Account_Success>();

            LoginAs<RogerReporter>();

            Click("Register an organisation");

            ExpectHeader("Registration Options");

            ClickLabel("Public Sector Organisation");

            Click("Continue");

            ExpectHeader("Find your organisation");

            Set("Find").To(Registration.OrgName_Blackpool);
            Click("Search");


            ExpectRow("Organisation name and registered address");
            ExpectRow(That.Contains, Registration.OrgName_Blackpool);
            ExpectRow(That.Contains, Registration.RegisteredAddress_Blackpool);

            //message should not appear with single result 
            ExpectNo(What.Contains, "Showing 1-");


            AtRow(That.Contains, Registration.OrgName_Blackpool).Click("Choose Organisation");

            ExpectHeader("Address of the organisation you`re reporting for");
            ExpectText("Enter the correspondence address for the organisation whose Modern Slavery statement you’re reporting.");

            //fields pre-populated
            AtField("Address 1").Expect(Registration.Address1_Blackpool);
            AtField("Address 2").Expect(Registration.Address2_Blackpool);
            AtField("Address 3").Expect(Registration.Address3_Blackpool);
            AtField("Postcode").Expect(Registration.PostCode_Blackpool);

            Click("Continue");

            ExpectHeader("Your contact details");
            ExpectText("Please enter your contact details. The Government Equalities Office may contact you to confirm your registration.");

            //fields pre-populated
            AtField("First name").Expect(create_account.roger_first);
            AtField("Last name").Expect(create_account.roger_last);
            AtField("Email address").Expect(create_account.roger_email);
            AtField("Job title").Expect(create_account.roger_job_title);
            //todo confirm phone number field
            //AtField("Telephone").Expect();

            Click("Continue");

            ExpectHeader("Confirm your organisation’s details");

            AtLabel("Organisation name").Expect(Registration.OrgName_Blackpool);
            AtLabel("Registered address").Expect(Registration.RegisteredAddress_Blackpool);
            AtLabel("Business Sectors (SIC Codes)").Expect(Registration.SicCodes_Blackpool);

            ExpectHeader("Your contact details");
            AtLabel("Your name").Expect(create_account.roger_first + " " + create_account.roger_last);
            AtLabel("Email").Expect(create_account.roger_email);
            //todo confirm telephone
            //AtLabel("Telephone").Expect();
            AtLabel("").Expect(Registration.OrgName_Blackpool);

            ExpectHeader("Is this a UK address");
            ClickLabel("No");
            //should take you to the international workflow
            Click("Continue");

            //todo await confirmation of international workflow
        }
    }
}