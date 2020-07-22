using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Runtime.InteropServices;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class UserEditsDraftReportForCurrentYear : UITest
    {
        [TestMethod]
        public override void RunTest()
        {
            Run<UserCreatesADraftReport>();
            //LOG IN AS ORGNISATION MANAGER

            //Check landing page and navigation to target area
            ExpectHeader("Select an organisation");
            AtColumn("Organisation name").Click("Virgin Media");
            ExpectHeader("Manage your organisations reporting");

            //Make an edit to report
            AtRow("2020 to 2021").Expect("Contine");
            Click("Contine");
            ExpectHeader(That.Contains, "Review");
            Click("Back");
            ExpectHeader("Monitoring progress");
            Set("How is your orgnaisation measureing progress towards these goals?").To("Edited reasoning");

            //Check edited draft has saved
            Click("Contine");
            ExpectHeader(That.Contains, "Review");
            Click("Save draft");
            ExpectHeader("Select an organisation");
            AtColumn("Organisation name").Click("Virgin Media");
            ExpectHeader("Manage your organisations reporting");
            AtRow("2020 to 2021").Expect("Contine");
            Click("Contine");
            ExpectHeader(That.Contains, "Review");
            Click("Back");
            ExpectHeader("Monitoring progress");
            Expect("Edited reasoning");



        }
    }
}