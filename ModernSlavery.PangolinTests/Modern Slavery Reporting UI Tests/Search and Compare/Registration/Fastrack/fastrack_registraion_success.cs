﻿using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestCategory("Fasttrack")]
    [TestClass]
    public class Fastrack_Registraion_Success : UITest
    {
        [TestMethod]
        public override void RunTest()

        {
            //Run<RogerReportingUserCreatesAccount>();

            LoginAs<RogerReporter>();

            Click("Register an organisation");


            ExpectHeader("Registration Options");

            ClickLabel("Fast Track");
            Click("Continue");

            ExpectHeader("Fast track registration");

            //register organisation
            Set("Employer reference").To(Registration.EmployerReference_Milbrook);
            Set("Security code").To(Registration.SecurtiyCode_Millbrook);

            Click("Continue");


            ExpectHeader("Confirm your organisation’s details");

            //expect organisation details
            AtRow("Organisation name").Expect(Registration.OrgName_Millbrook);
            AtRow("Company number").Expect(Registration.CompanyNumber_Millbrook);
            AtRow("Registered address").Expect(Registration.RegisteredAddress_Millbrook);
            //AtRow("Business Sectors (SIC Codes)").Expect("");

            Click("Confirm");
            ExpectHeader("You can now publish a Modern Slavery statement on behalf of this organisation.");

            //At("Employer name").Expect("MILLBROOK HEALTHCARE LTD");

            //Below("Employer name").ExpectText("You can also specify whether this employer is in scope of the reporting regulations.");

            ClickButton("Continue");

            ExpectHeader("Select an organisation");

            ExpectRow("Organisation name");
            AtRow("Organisation name").Column("Organisation Status").Expect("Registration Complete");

        }


    }
}