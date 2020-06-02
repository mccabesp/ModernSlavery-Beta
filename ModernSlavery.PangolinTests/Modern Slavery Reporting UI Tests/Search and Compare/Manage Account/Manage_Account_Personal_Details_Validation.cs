using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class Manage_Account_Personal_Details_Validation : UITest
    {
        [TestMethod]
        public override void RunTest()
        {
            Run<Create_Account_Success>();

            LoginAs<RogerReporter>();

            Click("Manage Account");

            ExpectHeader("Login details");

            Click(The.Bottom, "Change");
            ExpectHeader("Change your personal details");

            AtField("First name").Expect(create_account.roger_first);
            AtField("Last name").Expect(create_account.roger_last);
            AtField("Job title").Expect(create_account.roger_job_title);

            ClearField("First name");
            ClearField("Last name");
            ClearField("Job title");

            Click("Continue");

            Expect("The following errors were detected");
            Expect("You need to enter your first name");
            Expect("You need to enter your last name");
            Expect("You need to enter your job title");

        }
    }
}