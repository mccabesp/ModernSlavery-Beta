using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class ManageAccount_Change_Email : UITest
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

            Set("Email address").To(create_account.edited_email);
            Set("Confirm email address").To(create_account.edited_email);

            Click("Confirm");
            Expect("Your details have been updated successfully");

            Click("Manage Account");
            ExpectHeader("Manage your account");

            AtRow("Email address").Expect(create_account.edited_email);

        }
    }
}