using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class FastrackRegistrationMandatoryFieldsCheck : UITest
    {
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

            //validation messages to be confirmed
            Expect("You must enter an employer reference");
            Expect("You must enter a security code");            
        }
    }
}