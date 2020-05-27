using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class Manage_Account_Email_Validation : UITest
    {
        [TestMethod]
        public override void RunTest()
        {
            Run<Create_Account_Success>();

            LoginAs<RogerReporter>();

            Click("Manage Account");

            ExpectHeader("Login details");

            Click(The.Top, "Change");

            ExpectHeader("Enter your new email address");

            //mandatory field

            ClearField("Email address");
            ClearField("Confirm your email address");

            Click("Confirm");
            Expect("You need to provide an email address");

            //emails must match
            Set("Email address").To("1@test.com");
            Set("Confirm your email address").To("2@test.com");

            Click("Confirm");

            Expect("There is a problem");
            Expect("The email address and confirmation do not match");

            //cannot use existing email address
            Set("Email address").To(create_account.existing_email);
            Set("Confirm your email address").To(create_account.existing_email);

            Click("Confirm");

            Expect("The following errors were detected");
            Expect("This email address has already been registered. Please enter a different email address or request a password reset.");


        }
    }
}