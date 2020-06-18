using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class Registration_Mandatory_Organisation_Type : UITest
    {
        [TestCategory("Fasttrack")]
        [TestMethod]
        public override void RunTest()
        {
            //Run<RogerReportingUserCreatesAccount>();
            LoginAs<RogerReporter>();

            Click("Register an organisation");


            ExpectHeader("Registration Options");


            //clicking continue without selecting a route throws error.
            Click("Continue");

            Expect("The following errors were detected");
            Below("The following errors were detected").Expect("There’s a problem");

            Expect("Please choose your type of organisation");

        }
    }
}