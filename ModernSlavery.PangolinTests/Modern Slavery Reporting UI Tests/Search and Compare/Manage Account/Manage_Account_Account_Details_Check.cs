using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class Manage_Account_Account_Details_Check : UITest
    {
        [TestMethod]
        public override void RunTest()
        {
            Run<Create_Account_Success>();

            LoginAs<RogerReporter>();

            Click("Manage Account");

            ExpectHeader("Login details");

            AtRow("Email address").Expect(create_account.roger_email);
            AtRow("Password").Expect("********");

            ExpectHeader("Personal details");
            AtRow("First name").Expect(create_account.roger_first);
            AtRow("Last name").Expect(create_account.roger_last);
            AtRow("Job title").Expect(create_account.roger_job_title);
            
            //todo find out about phone number field
            AtRow("Phone number").Expect("");




        }
    }
}