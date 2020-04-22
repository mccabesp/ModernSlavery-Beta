using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class CreateAnAccountValidationCheck : UITest
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

            //invalid email addess
            Set("Email address").To("invalid");
            Set("")
        }
    }
}