using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class Your_Modern_Slavery_Statement_Date_Validation : UITest
    {
        [TestMethod]
        public override void RunTest()
        {
            Run<Fastrack_Registration_Success>();

            LoginAs<RogerReporter>();

            ExpectHeader("Select an organisation");

            Click(Submission.OrgName_Blackpool);

            ExpectHeader("Manage your organisations reporting");

            //to after from
            Submission_Helper.DateSet(this, "2", "2", "2020", "1");
            Submission_Helper.DateSet(this, "2", "2", "2019", "2");

            Click("Continue");
            Expect("There is a problem");
            RefreshPage();


            //invalid format
            Submission_Helper.DateSet(this, "22", "12", "2020", "3");
            Click("Continue");
            Expect("There is a problem");
            Expect("Date format is incorrect");

            RefreshPage();
        }
    }
}