using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class ThreeRepeatedFaileRegistrationAttemptsFreezeUser : UITest
    {
        [TestMethod]
        public override void RunTest()
        {
            LoginAs<UnregisteredUser>();
            Expect("Search and compare");

            Click("Sign in");
            ExpectHeader("Sign in");

            Set("Email").To("notregistered@uat.co");
            Set("Password").To("test");

            ExpectHeader("Sign in");
            //validation for non-registered user
            Expect("There's a problem with your email address or password");

            Set("Email").To("notregistered@uat.co");
            Set("Password").To("test");

            ExpectHeader("Sign in");
            //validation for non-registered user
            Expect("There`s a problem with your email address or password");


            Set("Email").To("notregistered@uat.co");
            Set("Password").To("test");

            ExpectHeader("Sign in");
            //after three failed attempts also display please try again message
            Expect("There`s a problem with your email address or password");
            Expect("Too many failed sign in attempts");

            //ommiting seconds for now due to uneven test case duration
            //todo add second check for this workflow
            Expect(What.Contains, "Please try again in 29 minutes and ");
        }
    }
}