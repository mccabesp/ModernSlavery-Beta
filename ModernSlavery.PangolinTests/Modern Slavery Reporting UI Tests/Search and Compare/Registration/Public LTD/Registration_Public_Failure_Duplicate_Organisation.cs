using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class Registration_Public_Failure_Duplicate_Organisation : UITest
    {
        [TestMethod]
        public override void RunTest()
        {
            Run<Create_Account_Success>();
            Run<Registration_Public_Success>();

            LoginAs<RogerReporter>();

            Click("Register an organisation");

            ExpectHeader("Registration Options");

            ClickLabel("Public Sector Organisation");

            Click("Continue");

            ExpectHeader("Find your organisation");

            SetXPath("//input[@id='SearchText']").To(Registration.OrgName_Blackpool);
            Click(The.Bottom, "Search");


            ExpectRow("Organisation name and registered address");
            ExpectRow(That.Contains, Registration.OrgName_Blackpool);
            ExpectRow(That.Contains, Registration.RegisteredAddress_Blackpool);



            ClickButton(That.Contains, "Choose");

            //org already registered
            //error should appear

            Expect("The following errors were detected");
            Expect("You have already registered this organisation");
        }
    }
}