using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class Submission_Six_Areas_Mandatory_Field_Check : UITest
    {
        [TestMethod]
        public override void RunTest()
        {
            Run<Fastrack_Registraion_Success>();

            LoginAs<RogerReporter>();

            ExpectHeader("Select an organisation");

            Click(Submission.OrgName_Blackpool);

            ExpectHeader("Manage your organisations reporting");

            Click("Draft Report");

            ExpectHeader("Your modern slavery statement");
            Set(The.Top, "Date").To(Submission.DateFrom_Blackpool);
            Set("First name").To(Submission.FirstName_Blackpool);
            Set("Last name").To(Submission.LastName_Blackpool);
            Set("Job title").To(Submission.JobTitle_Blackpool);
            Set("Url").To(Submission.Url_Blackpool);
            Set(The.Bottom, "Date").To(Submission.DateApproved_Blackpool);

            Click("Save and continue");

            ExpectHeader("Six areas of modern slavery statement");

            Click("Save and continue");

            //mandatory field markers should now appear
            //todo confirm if warning appears here on on draft submission
        }
    }
}