using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class Fastrack_Registration_Mandatory_Fields : UITest
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

            //clicking continue without fields filled in should trigger validation
            Click("Continue");

            ExpectHeader("There is a problem");
            AtHeader("There is a problem").Expect("You must enter an employer reference");
            AtHeader("There is a problem").Expect("You must enter a security code");

            AtField("Employer reference").Expect("You must enter an employer reference");
            AtField("Security code").Expect("You must enter a security code");



            //no employer reference
            Set("Security Code").To(Registration.SecurtiyCode_Millbrook);

            Click("Continue");
            AtHeader("There is a problem").Expect("You must enter an employer reference");
            AtField("Employer reference").Expect("You must enter an employer reference");


            //no security code
            ClearField("Security code");
            Set("Employer reference").To(Registration.EmployerReference_Milbrook);
            AtField("Security code").Expect("You must enter a security codeduplicate");


        }
    }
}