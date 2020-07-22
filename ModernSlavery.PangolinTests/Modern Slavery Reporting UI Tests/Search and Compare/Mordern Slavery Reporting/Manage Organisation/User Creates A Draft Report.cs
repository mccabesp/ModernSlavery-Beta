using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class UserCreatesADraftReport : UITest
    {
        [TestMethod]
        public override void RunTest()
        {
            //LOG IN AS ORGNISATION MANAGER

            //Check landing page and navigation to target area
            ExpectHeader("Select an organisation");
            AtColumn("Organisation name").Click("Virgin Media");
            ExpectHeader("Manage your organisations reporting");
            AtRow("2020/21").Click("Start draft");

            ExpectHeader("Before you start");
            Click("Start now");

            //Try to save draft without filling in any fields
            Click("Continue");
            Click("Continue");
            Click("Continue");
            Click("Continue");
            Click("Continue");
            Click("Continue");
            Click("Continue");
            Click("Continue");
            ExpectHeader(That.Contains, "Review");
            Click("Save draft");
            ExpectNoHeader("Select an organisation");
            // EXPECT IT TO FAIL MESSAGE TO BE CONFIRMED

            Click("Back");
            Set("How is your organisation measuring progress towards these goals?").To("Here is how we are doing this");
            Click("Continue");
            ExpectHeader(That.Contains, "Review");
            Click("Save Draft");
            ExpectHeader("Select an organisation");
            AtColumn("Organisation name").Click("Virgin Media");
            ExpectHeader("Manage your organisations reporting");

            AtRow("2020/21").Expect("Draft");
            AtRow("2020/21").Expect("Continue");




        }
    }
}