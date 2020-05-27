using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestCategory("Spam Protection")]
    [TestClass]
    public class Fastrack_Registraion_Spam_Protection_Reset_Count : UITest
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
            Set("Employer reference").To("Invalid1");
            Set("Security code").To("Invalid1");
            Click("Continue");

            ExpectHeader("Fast track registration");
            //validation for non-registered user
            Expect("There's a problem with your employer reference or security code");


            ExpectHeader("Fast track registration");

            //register organisation
            Set("Employer reference").To("Invalid2");
            Set("Security code").To("Invalid2");
            Click("Continue");

            ExpectHeader("Fast track registration");
            //validation for non-registered user
            Expect("There's a problem with your employer reference or security code");

            ExpectHeader("Fast track registration");

            //reset count with valid registration
            Set("Employer reference").To(Registration.EmployerReference_NationalHeritage);
            Set("Security code").To(Registration.SecurtiyCode_NationalHeritage);

            Click("Continue");
            ExpectHeader("Confirm your organisation’s details");


            Click("Confirm");
            ExpectHeader("You can now publish a Modern Slavery statement on behalf of this organisation.");


            Click("Submit");
            Click("Register an organisation");
            //registration should not now time out
            Click("Register an organisation");


            ExpectHeader("Registration Options");

            ClickLabel("Fast Track");
            Click("Continue");

            ExpectHeader("Fast track registration");

            //register organisation
            Set("Employer reference").To("Invalid3");
            Set("Security code").To("Invalid3");
            Click("Continue");

            ExpectNo("Too many attempts");


        }
    }
}