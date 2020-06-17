using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class Sign_Out : UITest
    {
        [TestMethod]
        public override void RunTest()
        {
            //Run<Create_Account_Success>();

            LoginAs<RogerReporter>();

            ExpectNo("Sign in");

            Click("Sign out");


            //this section has been changed for MVP 1
            //Expect("Signed Out");

            ////text split so multiple expects requrired
            //Expect(What.Contains, "You are now signed out of the");
            //Expect(What.Contains, "Modern Slavery reporting service");
            //Expect(What.Contains, "Click the button below to return to the ");
            //Expect(What.Contains, "Modern Slavery reporting service");
            //Click("Continue");

            //ExpectHeader("Search and compare Modern Slavery statements");

            Expect("You are signed out");

            ClickButton("Continue");
        }
    }
}