using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class CannotRegisterOrganisationWithExpiredSecurityCode : UITest
    {
        [TestMethod]
        public override void RunTest()
        {
            LoginAs<RogerReporter>();

            Click("Register an organisation");

            ExpectHeader("Registration Options");

            ClickLabel("Fast Track");
            Click("Continue");

            ExpectHeader("Fast track registration");

            //validation message to appear with expired security code
            Set("Employer reference").To(Fastrack.ValidEmployerReference);
            Set("Security code").To(Fastrack.ExpiredSecurityCode);

            Click("Continue");

            Expect("Your Security Code has expired");
        }
    }
}