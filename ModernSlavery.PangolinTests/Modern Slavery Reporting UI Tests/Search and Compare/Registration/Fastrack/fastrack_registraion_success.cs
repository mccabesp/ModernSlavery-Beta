using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestCategory("Fasttrack")]
    [TestClass]
    public class Fastrack_Registration_Success : UITest
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
            
            //using contains due to label including encoded spaces and not being detected properly
            AtRow(That.Contains, "Business").Expect(Registration.SicCode_Milbrook.Item1);
            AtRow(That.Contains, "Business").Below(Registration.SicCode_Milbrook.Item1).Expect(Registration.SicCode_Milbrook.Item2);
            AtRow(That.Contains, "Business").RightOf(Registration.SicCode_Milbrook.Item2).Expect(Registration.SicCode_Milbrook.Item3);

            Click("Confirm");
            ExpectHeader("You can now publish a Modern Slavery statement on behalf of this organisation.");

            At("Employer name").Expect(Registration.OrgName_Millbrook);

            Below("Employer name").ExpectText("You can also specify whether this employer is in scope of the reporting regulations.");

            Click("Continue");

            ExpectHeader("Select an organisation");

            ExpectRow(Registration.OrgName_Millbrook);
            AtRow(Registration.OrgName_Millbrook).Column("Organisation Status").Expect("Registration Complete");

        }


    }
}