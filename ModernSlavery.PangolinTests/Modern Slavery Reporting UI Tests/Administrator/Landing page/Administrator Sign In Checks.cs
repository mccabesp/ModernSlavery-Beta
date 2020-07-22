using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class AdministratorSignInChecks : UITest
    {
        [TestMethod]
        public override void RunTest()
        {
            //Run test to log in as administator user
            ExpectHeader("Administrator");

            //Expect Search content
            ExpectHeader("Search");
            Expect("Organisation:");
            Expect("current name, previous name, employer regerence, company number");

            Expect("User:");
            Expect("name, email address");

            //Actions
            ExpectHeader("Actions");
            Expect("Impersonate User");
            Expect("Execute manual changes");

            //Registrations
            ExpectHeader("Registrations");
            Expect("Unconfirmed PINs in the post");
            Expect("Pending registrations");

            //Information
            ExpectHeader("Information");
            Expect("History");
            Expect("Downloads");
            Expect("Uploads");

            //Logs
            ExpectHeader("Logs");
            Expect("Download feedback");
            Expect("Web UI logs");
            Expect("Webjob logs");
            Expect("Identity Server logs");







        }
    }
}