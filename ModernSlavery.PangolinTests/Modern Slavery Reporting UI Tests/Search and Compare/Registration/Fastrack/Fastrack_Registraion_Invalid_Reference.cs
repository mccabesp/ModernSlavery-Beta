using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class Fastrack_Registration_Invalid_Reference : UITest
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

            //todo ensure valid security code added here
            Set("Employer reference").To(Registration.InvalidEmployerReference);
            Set("Security code").To(Registration.ValidSecurityCode);

            Click("Continue");

            ExpectHeader("There is a problem");
            Expect("There's a problem with your employer reference or security code");
        }
    }
}