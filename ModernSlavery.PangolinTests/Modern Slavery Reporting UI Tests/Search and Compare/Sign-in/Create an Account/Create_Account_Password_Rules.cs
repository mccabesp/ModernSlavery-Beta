using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class Create_Account_Password_Rules : UITest
    {
        
        [TestMethod ]
        

        public override void RunTest()
        {
            Logout();
            Goto("/");

            Click("Sign in");

            ExpectHeader("Sign in");

            BelowHeader("No account yet?");
            Click("Create an account");


            ExpectHeader("Create an Account");

            Set("Email address").To("roger@test.co");
            Set("Confirm your email address").To("roger@test.co");

            Set("First name").To("Roger");
            Set("Last Name").To("Reporter");

            Set("Job title").To("Company Reporter");

            Set("Password").To("Test1234!");
            Set("Confirm Password").To("Test1234!");

            ClickLabel("I would like to receive information about webinars, events and new guidance");
            ClickLabel("I'm happy to be contacted for feedback on this service and take part in Modern Slavery surveys");

            Click("Continue");

            ExpectHeader("Verify your email address");
            Expect("We have sent a confirmation email to");
            Expect("roger@test.co");
            Expect("Follow the instructions in the email to continue your registration.");

        }
    }
}