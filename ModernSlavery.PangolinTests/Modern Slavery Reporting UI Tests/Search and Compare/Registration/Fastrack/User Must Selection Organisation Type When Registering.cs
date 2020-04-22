using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class UserMustSelectionOrganisationTypeWhenRegistering : UITest
    {
        [TestMethod]
        public override void RunTest()
        {
            Run<RogerReportingUserCreatesAccount>();
            LoginAs<RogerReporter>();

            Click("Register an organisation");


            ExpectHeader("Registration Options");


            //clicking continue without selecting a route throws error.
            Click("Continue");

            ExpectHeader("The following errors were detected");
            BelowHeader("The following errors were detected").Expect("There's a problem");

        }
    }
}