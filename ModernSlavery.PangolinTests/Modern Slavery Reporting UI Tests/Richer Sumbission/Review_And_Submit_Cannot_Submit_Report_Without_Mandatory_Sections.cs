using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class Review_And_Submit_Cannot_Submit_Report_Without_Mandatory_Sections : UITest
    {
        [TestMethod]
        public override void RunTest()
        {
            LoginAs<RogerReporter>();

            Submission_Helper.NavigateToSubmission(this, Submission.OrgName_Blackpool, "2020", "2021");

            Expect("Submission incomplete. Section 1 must be completed in order to submit.");

            AtRow("Your modern slavery statment").Expect("Not completed");
            AtRow("Areas covered by your modern slavery statement").Expect("Not completed");

            //comple your modern slavery statement

            Click("Your modern slavery statement");
            ExpectHeader("Your modern slavery statement");

            Set("URL").To(Submission.YourMSStatement_URL);

            Submission_Helper.DateSet(this, Submission.YourMSStatement_To_Day, Submission.YourMSStatement_To_Month, Submission.YourMSStatement_To_Year, "1");
            Submission_Helper.DateSet(this, Submission.YourMSStatement_To_Day, Submission.YourMSStatement_To_Month, Submission.YourMSStatement_To_Year, "2");

            Set("First name").To(Submission.YourMSStatement_First);
            Set("Last name").To(Submission.YourMSStatement_Last);
            Set("Job title").To(Submission.YourMSStatement_JobTitle);

            Submission_Helper.DateSet(this, Submission.YourMSStatement_ApprovalDate_Day, Submission.YourMSStatement_ApprovalDate_Month, Submission.YourMSStatement_ApprovalDate_Year, "3");

            Click("Continue");

            ExpectHeader("Review 2019 to 2020 group report for " + Submission.OrgName_Blackpool);

            //both pages need completed in order to continue
            Expect("Submission incomplete. Section 1 must be completed in order to submit.");

            AtRow("Your modern slavery statment").Expect("Completed");
            AtRow("Areas covered by your modern slavery statement").Expect("Not completed");

            Click("Areas covered by your modern slavery statement");

            ExpectHeader("Areas covered by your modern slavery statement");

            AtLabel("Your organisation’s structure, business and supply chains").ClickLabel("Yes");
            AtLabel("Policies").ClickLabel("Yes");
            AtLabel("Risk assessment and management").ClickLabel("Yes");
            AtLabel("Due diligence processes").ClickLabel("Yes");
            AtLabel("Staff training about slavery and human trafficking").ClickLabel("Yes");
            AtLabel("Goals and key performance indicators (KPIs) to measure your progress over time, and the effectiveness of your actions").ClickLabel("Yes");

            Click("Continue");

            ExpectHeader("Review 2019 to 2020 group report for " + Submission.OrgName_Blackpool);

            //both pages complete now
            
            ExpectNo("Submission incomplete. Section 1 must be completed in order to submit.");

            AtRow("Your modern slavery statment").Expect("Completed");
            AtRow("Areas covered by your modern slavery statement").Expect("Completed");
        }
    }
}