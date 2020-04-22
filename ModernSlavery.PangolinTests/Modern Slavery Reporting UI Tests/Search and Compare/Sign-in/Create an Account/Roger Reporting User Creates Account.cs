using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class RogerReportingUserCreatesAccount : UITest
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

            ExpectHeader("Verify your email address");

            Expect("We have sent a confirmation email to");
            Expect("roger@uat.co");
            Expect("Follow the instructions in the email to continue your registration");

        }
    }
}