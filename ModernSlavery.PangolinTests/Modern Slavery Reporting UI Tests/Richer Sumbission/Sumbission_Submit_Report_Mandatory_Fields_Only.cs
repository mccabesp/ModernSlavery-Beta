using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class Sumbission_Submit_Report_Mandatory_Fields_Only : UITest
    {
        [TestMethod]
        public override void RunTest()
        {
            Run<Submission_Complete_Mandatory_Sections>();

            LoginAs<RogerReporter>();

            Submission_Helper.NavigateToSubmission(this, Submission.OrgName_Blackpool, "2020", "2021");

            //mandatory sections should be completed
            AtRow("Your modern Slavery statement").Expect("Completed");
            AtRow("Areas covered by your modern slavery statement").Expect("Completed");

            //all other sections incomplete 

            AtRow("Your organisation").Expect("Not Completed");
            AtRow("Policies").Expect("Not Completed");
            AtRow("Supply chain risks and due diligence (part 1)").Expect("Not Completed");
            AtRow("Supply chain risks and due diligence (part 2)").Expect("Not Completed");
            AtRow("Training").Expect("Not Completed");
            AtRow("Monitoring progress").Expect("Not Completed");

            Click("Confirm and submit");

            Expect("You've submitted your Modern Slavery statement for 2019 to 2020");

            Click("Finish and Sign out");


            WaitForNewPage();



        }
    }
}