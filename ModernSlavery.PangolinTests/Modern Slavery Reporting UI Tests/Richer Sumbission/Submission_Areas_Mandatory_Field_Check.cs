using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class Submission_Areas_Mandatory_Field_Check : UITest
    {
        [TestMethod]
        public override void RunTest()
        {
            Run<Fastrack_Registration_Success>();

            LoginAs<RogerReporter>();

            ExpectHeader("Select an organisation");

            Click(Submission.OrgName_Blackpool);


            ExpectHeader("Manage your organisations reporting");

            Click("Draft Report");


            ExpectHeader("Before you start");
            Click("Start now");

            ExpectHeader("Your modern slavery statement");

            Click("Save and continue");

            ExpectHeader("Areas covered by your modern slavery statement");


            AtLabel("Your organisation’s structure, business and supply chains").ClickLabel("Yes");

            Click("Save and continue");

            ExpectHeader("There is a problem");
            Expect("Please provide the detail");

            //inline error tbc
            AtLabel("Please provide details").Expect("Please provide the detail");
        }
    }
}