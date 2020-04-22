using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class SignInPageContentCheck : UITest
    {
        [TestMethod]
        public override void RunTest()
        {
            LoginAs<UnregisteredUser>();
            Click("Sign in");
            ExpectHeader("Sign in");

            AboveHeader("Sign in").Expect(What.Contains, "This is a new service - your");
            AboveHeader("Sign in").ExpectLink("Feedback");
            AboveHeader("Sign in").Expect(What.Contains, " will help us improve it");



        }
    }
}