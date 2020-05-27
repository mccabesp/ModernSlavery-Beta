using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class Registration_Public_Start_Reigstration : UITest
    {
        [TestMethod]
        public override void RunTest()
        {

            //statrt registering organisation for testing workflows involving started registration process
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
        }
    }
}