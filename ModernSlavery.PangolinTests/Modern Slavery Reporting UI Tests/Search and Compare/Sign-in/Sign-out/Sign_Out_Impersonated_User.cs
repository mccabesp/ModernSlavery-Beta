using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class Sign_Out_Impersonated_User : UITest
    {
        [TestCategory("Unwritten")]
        [TestMethod]
        public override void RunTest()
        {
            //Login as admin user
            ExpectHeader("Administrator");
            ExpectHeader("Actions");
            Expect("Impersonate User");

            Click("Impersonate User");
            ExpectHeader("Administration");
            Expect("Impersonation");
            Set("email address").To("stephen@coolcorner.com");

            Expect("Select an organsation");
            Click("Sign out");
            ExpectNo("You are now signed out of the Submit or view a modern slavery statement service");
            ExpectHeader("Administrator");

        }
    }
}