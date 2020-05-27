using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class Close_Account_Invalid_Password : UITest
    {
        [TestMethod]
        public override void RunTest()
        {
            Run<Create_Account_Success>();

            LoginAs<RogerReporter>();

            Click("Manage account");
            ExpectHeader("Manage your account");
            Click("Close your account");

            ExpectHeader("Close your account");

            Set("Enter your password to confirm").To("invalid");
            Click("Close your account");

            ExpectHeader("There is a problem");
            Expect("Could not verify your current password");
        }
    }
}