using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class Create_Account_Validation_Check : UITest
    {
        [TestMethod]
        public override void RunTest()
        {
            Goto("/");

            Click("Sign in");

            ExpectHeader("Sign in");

            BelowHeader("No account yet?");
            Click("Create an account");

            ExpectHeader("Create an Account");

            //invalid email address
            Set("Email address").To("invalid");
            Click("Continue");
            Expect("There`s a problem with your email address");

            Expect("Please include an '@' in the email address. 'invalid' is missing an '@'.");

            //different email addresses
            Set("Email address").To("test@test.test");
            Set("Confirm email address").To("test2@test.test");
            Click("Continue");

            Expect("The following errors were detected");
            Expect("The email address and confirmation do not match");

            Set("Confirm email address").To("test@test.test");

            Click("Continue");

            //personal details
            //leave blank to test validaition
            //todo check validation messages
            Expect("The following errors were detected");
            Expect("You need to provide a first name");
            Expect("You need to provide a last name");
            Expect("You need to provide a job title");


        }
    }
}