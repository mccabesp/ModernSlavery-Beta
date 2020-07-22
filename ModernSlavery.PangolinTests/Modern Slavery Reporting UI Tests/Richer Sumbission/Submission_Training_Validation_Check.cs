using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class Submission_Training_Validation_Check : UITest
    {
        [TestMethod]
        public override void RunTest()
        {
            //Run<Fastrack_Registration_Success>();

            LoginAs<RogerReporter>();

            Submission_Helper.NavigateToTraining(this, Submission.OrgName_Blackpool, "2019/2020");

            ExpectHeader("Training");

            //if chosing "other" details must be provided
            ClickLabel("Other");
            ClearField("Please specify");

            Click("Save and continue");

            Expect("There is a problem");
            Expect("Please provide `other` details");

            Set("Please specify").To("details");
            Click("Save and continue");
            ExpectHeader("Monitoring progress");
        }
    }
}