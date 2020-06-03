using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class Sign_In_Failure_Closed_Account : UITest
    {
        [TestMethod]
        public override void RunTest()
        {
            //cannot sign in after closing account
            Run<Close_Account>();

            Logout();

            Goto("/");

            Goto("/");
            Click("Sign in");
            Set("Email").To(create_account.roger_email);
            Set("Password").To(create_account.roger_password);

            Click(The.Bottom, "Sign in");
            Expect("There`s a problem with your email address or password");
        }
    }
}