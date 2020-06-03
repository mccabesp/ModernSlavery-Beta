using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class Create_Account_Success : UITest
    {
        [TestMethod]
        public override void RunTest()
        {
            //succesful create account journey 
            Goto("/");

            Click("Sign in");

            ExpectHeader("Sign in");

            BelowHeader("No account yet?");
            Click("Create an account");

            ExpectHeader("Create an Account");

            Set("Email address").To(create_account.roger_email);
            Set("Confrim your email address").To(create_account.roger_email);

            Set("First name").To(create_account.roger_first);
            Set("Last name").To(create_account.roger_last);
            Set("Job title").To(create_account.roger_job_title);

            Set("Pasword").To(create_account.roger_password);
            Set("Confirm password").To(create_account.roger_password);

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