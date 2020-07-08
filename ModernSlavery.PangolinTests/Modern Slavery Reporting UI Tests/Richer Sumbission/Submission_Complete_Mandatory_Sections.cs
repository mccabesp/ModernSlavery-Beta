using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class Submission_Complete_Mandatory_Sections : UITest
    {
        [TestMethod]
        public override void RunTest()
        {
            //Run<Fastrack_Registration_Success>();

            LoginAs<RogerReporter>();

            ExpectHeader("Select an organisation");

            Click(Submission.OrgName_Blackpool);

            ExpectHeader("Manage your organisations reporting");

            AtRow("2019/20").Click("Draft report");

            ExpectHeader("Before you start");
            Click("Start Now");

            ExpectHeader("Your modern slavery statement");

            Set("URL").To(Submission.YourMSStatement_URL);

            Submission_Helper.DateSet(this, Submission.YourMSStatement_To_Day, Submission.YourMSStatement_To_Month, Submission.YourMSStatement_To_Year, "1");
            Submission_Helper.DateSet(this, Submission.YourMSStatement_To_Day, Submission.YourMSStatement_To_Month, Submission.YourMSStatement_To_Year, "2");

            Set("First name").To(Submission.YourMSStatement_First);
            Set("Last name").To(Submission.YourMSStatement_Last);
            Set("Job title").To(Submission.YourMSStatement_JobTitle);

            Submission_Helper.DateSet(this, Submission.YourMSStatement_ApprovalDate_Day, Submission.YourMSStatement_ApprovalDate_Month, Submission.YourMSStatement_ApprovalDate_Year, "3");

            Click("Continue");

            ExpectHeader("Areas covered by your modern slavery statement");

            AtLabel("Your organisation’s structure, business and supply chains").ClickLabel("Yes");
            AtLabel("Policies").ClickLabel("Yes");
            AtLabel("Risk assessment and management").ClickLabel("Yes");
            AtLabel("Due diligence processes").ClickLabel("Yes");
            AtLabel("Staff training about slavery and human trafficking").ClickLabel("Yes");
            AtLabel("Goals and key performance indicators (KPIs) to measure your progress over time, and the effectiveness of your actions").ClickLabel("Yes");

            Click("Continue");

            ExpectHeader("Supply Chain Risks and due diligence");
        }
    }
}