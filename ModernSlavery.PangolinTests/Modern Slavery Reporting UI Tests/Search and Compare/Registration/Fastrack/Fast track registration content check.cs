using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class FastTrackRegistrationContentCheck : UITest
    {
        [TestMethod]
        public override void RunTest()
        {
            Run<RogerReportingUserCreatesAccount>();
            LoginAs<RogerReporter>();

            Click("Register an organisation");


            ExpectHeader("Registration Options");

            ClickLabel("Fast Track");

            TakeScreenshot("name");

            Click("Continue");

            ExpectHeader("Fast track registration");

            BelowHeader("Fast track registration").ExpectText("If you have received a letter you can enter your employer reference and security code to fast track your organisation`s registration");

            BelowHeader("Fast track registration").ExpectLabel("Employer reference");
            BelowHeader("Fast track registration").ExpectField("Employer reference");

            BelowHeader("Fast track registration").ExpectLabel("Security code");
            BelowHeader("Fast track registration").ExpectField("Security code");

            ExpectButton("Continue");



        }
    }
}