using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class Fastrack_Registraion_Invalid_Security : UITest
    {
        [TestCategory("Fasttrack")]
        [TestMethod]
        public override void RunTest()
        {
            //Run<RogerReportingUserCreatesAccount>();
            LoginAs<RogerReporter>();

            Click("Register an organisation");


            ExpectHeader("Registration Options");

            ClickLabel("Fast Track");

            Click("Continue");

            ExpectHeader("Fast track registration");

            //todo ensure valid employer reference added here
            Set("Employer reference").To(Registration.ValidEmployerReference);
            Set("Security code").To(Registration.InvalidSecurityCode);

            Click("Continue");

            Expect("There's a problem with your employer reference or security code");

        }
    }
}