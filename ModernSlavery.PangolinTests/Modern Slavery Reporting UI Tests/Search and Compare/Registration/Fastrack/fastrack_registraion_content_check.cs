using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class Fastrack_Registraion_Content_Check : UITest
    {
        [TestCategory("Fasttrack")]
        [TestMethod]
        public override void RunTest()
        {
            LoginAs<RogerReporter>();

            Click("Register an organisation");


            ExpectHeader("Registration Options");

            ClickLabel("Fast Track");

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