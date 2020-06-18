using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class Registration_Private_DuplicateOrg : UITest
    {
        [TestMethod]
        public override void RunTest()
        {
            Run<Registration_Private_Success>();

            LoginAs<RogerReporter>();

            LoginAs<RogerReporter>();

            Click("Register an organisation");

            ExpectHeader("Registration Options");

            ClickLabel("Private Limited Company, Charity");

            Click("Continue");

            ExpectHeader("Find your organisation");

            Set("SearchText").To(Registration.OrgName_InterFloor);
            Click("Search");

            AtRow(Registration.OrgName_InterFloor).Click("Choose organisation");

            Expect("The following errors were detected");
            Expect("You have already registered this organisation");
        }
    }
}