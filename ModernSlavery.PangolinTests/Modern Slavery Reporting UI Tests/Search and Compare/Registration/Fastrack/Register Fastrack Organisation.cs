using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class RegisterFastrackOrganisation : UITest
    {
        [TestProperty("Sprint", "2")]
        [TestMethod]
        public override void RunTest()

        {
            //Run<RogerReportingUserCreatesAccount>();

            LoginAs<RogerReporter>();

            Click("Register an organisation");


            ExpectHeader("Registration Options");

            ClickLabel("Fast Track");
            Click("Continue");

            ExpectHeader("Fast track regitration");

            //register organisation
            Set("Employer reference").To(Fastrack.ValidEmployerReference);
            Set("Security code").To(Fastrack.ValidSecurityCode);

            Click("Continue");


            ExpectHeader("Confirm your organisation`s details");

            //expect organisation details
            AtLabel("Organisation name").Expect("");
            AtLabel("Company number").Expect("");
            AtLabel("Registered address").Expect("");
            AtLabel("Business Sectors (SIC Codes)").Expect("");

            Click("Confirm");
            ExpectHeader("You can now publish a Modern Slavery statement on behalf of this organisation.");

            AtLabel("Employer name").Expect("");

            BelowLabel("Employer name").ExpectText("You can also specify whether this employer is in scope of the reporting regulations.");

            ClickButton("Continue");

            ExpectHeader("Select an organisation");

            ExpectRow("Organisation name");
            AtRow("Organisation name").Column("Organisation Status").Expect("Registration Complete");

        }
    }
}