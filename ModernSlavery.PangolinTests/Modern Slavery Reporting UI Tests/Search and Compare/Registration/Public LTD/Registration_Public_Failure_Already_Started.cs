using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class Registration_Public_Failure_Already_Started : UITest
    {
        [TestMethod]
        public override void RunTest()
        {
            //Run<Create_Account_Success>();
           // Run<Registration_Public_Start_Reigstration>();

            //LoginAs<RogerReporter>();

            //Run<Create_Account_Success>();

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
            ExpectRow(That.Contains, "PO Box 4, Blackpool, Lancashire, United Kingdom, FY1 1NA");

            //message should not appear with single result 
            ExpectNo(What.Contains, "Showing 1-");


            ClickButton(That.Contains, "Choose");

            //org already started
            //error should appear

            Expect("The following errors were detected");
            Expect("You have already started registering for this organisation");
        }
    }
}