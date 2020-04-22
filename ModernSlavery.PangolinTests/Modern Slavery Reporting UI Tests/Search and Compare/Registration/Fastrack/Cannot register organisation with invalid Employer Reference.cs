using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class CannotRegisterOrganisationWithInvalidEmployerReference : UITest
    {
        [TestMethod]
        public override void RunTest()
        {
            Run<RogerReportingUserCreatesAccount>();
            LoginAs<RogerReporter>();

            Click("Register an organisation");


            ExpectHeader("Registration Options");

            ClickLabel("Fast Track");
            Click("Continue");

            ExpectHeader("Fast track regitration");

            //todo ensure valid security code added here
            Set("Employer reference").To(Fastrack.InvalidEmployerReference);
            Set("Security code").To(Fastrack.ValidSecurityCode);

            Click("Continue");

            Expect("You have entered an invalid employer reference or security code");
        }
    }
}