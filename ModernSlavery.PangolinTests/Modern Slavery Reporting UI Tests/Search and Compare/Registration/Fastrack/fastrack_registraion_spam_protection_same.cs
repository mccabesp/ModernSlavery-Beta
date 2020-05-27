using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestCategory("Spam Protection")]
    [TestClass]
    public class Fastrack_Registraion_Spam_Protection_Same : UITest
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

            //register organisation
            Set("Employer reference").To(Registration.ValidEmployerReference);
            Set("Security code").To(Registration.InvalidSecurityCode);
            Click("Continue");

            ExpectHeader("Fast track registration");
            //validation for non-registered user
            Expect("There's a problem with your employer reference or security code");


            ExpectHeader("Fast track registration");

            //register organisation
            Set("Employer reference").To(Registration.ValidEmployerReference);
            Set("Security code").To(Registration.InvalidSecurityCode);
            Click("Continue");

            ExpectHeader("Fast track registration");
            //validation for non-registered user
            Expect("There's a problem with your employer reference or security code");

            ExpectHeader("Fast track registration");

            //register organisation
            Set("Employer reference").To(Registration.ValidEmployerReference);
            Set("Security code").To(Registration.InvalidSecurityCode);
            Click("Continue");

            ExpectHeader("Fast track registration");

            //validation for non-registered user
            Expect("There's a problem with your employer reference or security code");

            ExpectHeader("Fast track registration");

            //register organisation
            Set("Employer reference").To(Registration.ValidEmployerReference);
            Set("Security code").To(Registration.InvalidSecurityCode);
            Click("Continue");

            Expect("Too many attempts");
            Expect("You've attempted this action too many times.");

            //ommiting seconds for now due to uneven test case duration
            //todo add second check for this workflow
            Expect(What.Contains, "Please try again in 29 minutes and ");
        }
    }
}