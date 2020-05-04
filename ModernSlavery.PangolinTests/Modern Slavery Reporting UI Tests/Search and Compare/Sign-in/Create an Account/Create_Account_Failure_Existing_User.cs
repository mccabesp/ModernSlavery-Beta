using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class Create_Account_Failure_Existing_Users : UITest
    {
        [TestMethod]
        public override void RunTest()
        {
            //register account with email already verified in system
            Goto("/");

            Click("Sign in");

            ExpectHeader("Sign in");

            BelowHeader("No account yet?");
            Click("Create an account");

            ExpectHeader("Create an Account");

            Set("Email address").To(create_account.existing_email);
            Set("Confrim your email address").To(create_account.existing_email);

            Set("First name").To("Existing");
            Set("Last name").To("User");
            Set("Job title").To("Reporter");

            Set("Pasword").To("Test1234");
            Set("Confirm password").To("Test1234");

            ClickLabel("I would like to receive information about webinars, events and new guidance");
            ClickLabel("I'm happy to be contacted for feedback on this service and take part in Modern Slavery surveys");

            Click("Continue");


            //todo check validation messages
            Expect("The following errors were detected");
            Expect("This email address has already been registered. Please enter a different email address or request a password reset.");
        }
    }
}