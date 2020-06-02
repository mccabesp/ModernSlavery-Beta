using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class Manage_Account_Change_Personal_Details : UITest
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

            Set("First Name").To(create_account.edited_first);
            Set("Last Name").To(create_account.edited_last);
            Set("Job title").To(create_account.edited_job_title);

            //todo confirm phone number presence
            Set("Phone number").To("123");

            ClickLabel("I would like to receive information about webinars, events and new guidance");
            ClickLabel("I'm happy to be contacted for feedback on this service and take part in Modern Slavery surveys");

            Click("Continue");

            Expect("Your details have been updated successfully");
            Click("Manage Account");

            AtRow("First Name").Expect(create_account.edited_job_title);
            AtRow("Last Name").Expect(create_account.edited_last);
            AtRow("Job title").Expect(create_account.edited_job_title);

            AtRow("I would like to receive information about webinars, events and new guidance").Expect("No");
            AtRow("I'm happy to be contacted for feedback on this service and take part in Modern Slavery surveys").Expect("No");

        }
    }
}