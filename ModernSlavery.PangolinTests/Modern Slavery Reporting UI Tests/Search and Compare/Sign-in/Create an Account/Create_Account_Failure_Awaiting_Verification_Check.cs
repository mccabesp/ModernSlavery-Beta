using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class Create_Account_Awaiting_Verification_Check : UITest
    {
        [TestMethod]
        public override void RunTest()
        {
            Run<Create_Account_Success>();

            //if creating account for email address used in last 24 hours validation should appear


            //register account with email roger@test.co again
            Goto("/");

            Click("Sign in");

            ExpectHeader("Sign in");

            BelowHeader("No account yet?");
            Click("Create an account");

            ExpectHeader("Create an Account");

            Set("Email address").To("roger@uat.co");
            Set("Confrim your email address").To("roger@uat.co");

            Set("First name").To("Roger");
            Set("Last name").To("Reporter");
            Set("Job title").To("Tester");

            Set("Pasword").To("Test1234");
            Set("Confirm password").To("Test1234");

            ClickLabel("I would like to receive information about webinars, events and new guidance");
            ClickLabel("I'm happy to be contacted for feedback on this service and take part in Modern Slavery surveys");

            Click("Continue");

            Expect("The following errors were detected");
            Expect("This email address is awaiting confirmation. Please enter a different email address or try again in 23 hours");
        }
    }
}