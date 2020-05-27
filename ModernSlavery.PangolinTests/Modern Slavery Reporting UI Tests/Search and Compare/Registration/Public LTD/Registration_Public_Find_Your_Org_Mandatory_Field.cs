using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class Registration_Public_Find_Your_Org_Mandatory_Field : UITest
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

            //clicking search without field filled should cause validaiton
            Click("Search");

            Expect("The following errors were detected");
            Expect("There's a problem with your search");
        }
    }
}