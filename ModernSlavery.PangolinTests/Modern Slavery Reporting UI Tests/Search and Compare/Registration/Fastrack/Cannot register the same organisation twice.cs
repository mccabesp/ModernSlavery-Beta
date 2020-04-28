using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class CannotRegisterTheSameOrganisationTwice : UITest
    {
        [TestMethod]
        public override void RunTest()
        {
            Run<RegisterFastrackOrganisation>();

            LoginAs<RogerReporter>();

            Click("Register an organisation");


            ExpectHeader("Registration Options");

            ClickLabel("Fast Track");
            Click("Continue");

            ExpectHeader("Fast track registration");

            //organisation already registered as part of pre-condition
            Set("Employer reference").To(Fastrack.ValidEmployerReference);
            Set("Security code").To(Fastrack.ValidSecurityCode);

            Click("Continue");

            Expect("You have already registered this organisation");
        }
    }
}