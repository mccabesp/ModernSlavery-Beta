using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class Close_Account_Mandatory_Fields : UITest
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

            Click("Close your account");

            ExpectHeader("There is a problem");
            Expect("You need to enter your pasword before you can close your account");
        }
    }
}