using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class Sign_Out_User_Cannot_Access_Registration_Pages : UITest
    {
        [TestMethod]
        public override void RunTest()
        {
            LoginAs<RogerReporter>();

            Click("Register an organisation");


            ExpectHeader("Registration Options");
            CopyUrl("registration");

            ClickLabel("Fast Track");
            Click("Continue");

            ExpectHeader("Fast track registration");
            CopyUrl("Fastrack");

            ExpectNo("Sign in");

            Click("Sign out");

            Expect("Signed Out");

            Click("Continue");

            ExpectHeader("Search and compare Modern Slavery statements");

            GotoCopiedUrl("registration");

            ExpectHeader("Sign in");

            GotoCopiedUrl("Fastrack");
            ExpectHeader("Sign in");
        }
    }
}