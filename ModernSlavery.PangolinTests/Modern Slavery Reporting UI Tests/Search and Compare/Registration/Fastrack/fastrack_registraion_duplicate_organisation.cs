using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class Fastrack_Registraion_Duplicate_Organisation : UITest
    {
        [TestCategory("Fasttrack")]
        [TestMethod]
        public override void RunTest()
        {
            //Run<Fastrack_Registraion_Success>();
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

            AtLabel("Employer name").Expect("MILLBROOK HEALTHCARE LTD");

            BelowLabel("Employer name").ExpectText("You can also specify whether this employer is in scope of the reporting regulations.");

            ClickButton("Continue");

            ExpectHeader("Select an organisation");

            ExpectRow("Organisation name");
            AtRow("Organisation name").Column("Organisation Status").Expect("Registration Complete");

            LoginAs<RogerReporter>();

            Click("Register an organisation");


            ExpectHeader("Registration Options");

            ClickLabel("Fast Track");
            Click("Continue");

            ExpectHeader("Fast track registration");

            //organisation already registered as part of pre-condition
            Set("Employer reference").To(Registration.ValidEmployerReference);
            Set("Security code").To(Registration.SecurtiyCode_Millbrook);

            Click("Continue");

            Expect("You have already registered this organisation");
        }
    }
}