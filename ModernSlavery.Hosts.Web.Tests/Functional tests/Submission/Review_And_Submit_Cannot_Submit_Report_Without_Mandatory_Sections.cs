using Geeks.Pangolin;
using ModernSlavery.Testing.Helpers.Extensions;
using NUnit.Framework;
using System.Threading.Tasks;

namespace ModernSlavery.Hosts.Web.Tests
{
    [TestFixture]

    public class Review_And_Submit_Cannot_Submit_Report_Without_Mandatory_Sections : Private_Registration_Success
    {
        [Test, Order(40)]
        public async Task NavigateToSubmissionPage()
        {
            Submission_Helper.NavigateToSubmission(this, Submission.OrgName_Blackpool, "2020 to 2021", MoreInfoRequired: true);
            await AxeHelper.CheckAccessibilityAsync(this).ConfigureAwait(false);
            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(42)]
        public async Task ExpectIncompleteText()
        {
            Expect("Submission incomplete. Section 1 must be completed in order to submit.");

            AtRow("Your modern slavery statment").Expect("Not completed");
            AtRow("Areas covered by your modern slavery statement").Expect("Not completed");

            //comple your modern slavery statement
            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(44)]
        public async Task NavigateToYourModernSlaveryStatement()
        {
            Click("Your modern slavery statement");
            await AxeHelper.CheckAccessibilityAsync(this).ConfigureAwait(false);
            ExpectHeader("Your modern slavery statement");
            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(46)]
        public async Task FillInStatementDetails()
        {
            Set("URL").To(Submission.YourMSStatement_URL);

            Submission_Helper.DateSet(this, Submission.YourMSStatement_To_Day, Submission.YourMSStatement_To_Month, Submission.YourMSStatement_To_Year, "1");
            Submission_Helper.DateSet(this, Submission.YourMSStatement_To_Day, Submission.YourMSStatement_To_Month, Submission.YourMSStatement_To_Year, "2");

            Set("First name").To(Submission.YourMSStatement_First);
            Set("Last name").To(Submission.YourMSStatement_Last);
            Set("Job title").To(Submission.YourMSStatement_JobTitle);

            Submission_Helper.DateSet(this, Submission.YourMSStatement_ApprovalDate_Day, Submission.YourMSStatement_ApprovalDate_Month, Submission.YourMSStatement_ApprovalDate_Year, "3");
            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(48)]
        public async Task ClickingContinueNavigatesBackToReviewwPage()
        {
            Click("Continue");
            await AxeHelper.CheckAccessibilityAsync(this).ConfigureAwait(false);
            ExpectHeader("Review 2020 group report for " + Submission.OrgName_Blackpool);
            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(50)]
        public async Task YourModernSlaverySectionCompleted()
        {

            AtRow("Your modern slavery statment").Expect("Completed");
            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(52)]
        public async Task BothSectionsNeedToBeCompletedInOrderToContinue()
        {
            //both pages need completed in order to continue

            AtRow("Areas covered by your modern slavery statement").Expect("Not completed");
            Expect("Submission incomplete. Section 1 must be completed in order to submit.");

            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(54)]
        public async Task ClickingAreasCoveredLabelLeadsToAreasCoveredPage()
        {
            Click("Areas covered by your modern slavery statement");
            await AxeHelper.CheckAccessibilityAsync(this).ConfigureAwait(false);
            ExpectHeader("Areas covered by your modern slavery statement");
            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(56)]
        public async Task FillInAreasCoveredDetails()
        {
            AtLabel("Your organisation’s structure, business and supply chains").ClickLabel("Yes");
            AtLabel("Policies").ClickLabel("Yes");
            AtLabel("Risk assessment and management").ClickLabel("Yes");
            AtLabel("Due diligence processes").ClickLabel("Yes");
            AtLabel("Staff training about slavery and human trafficking").ClickLabel("Yes");
            AtLabel("Goals and key performance indicators (KPIs) to measure your progress over time, and the effectiveness of your actions").ClickLabel("Yes");
            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(58)]
        public async Task AfterFillingMandatorySectionsDraftCanBeSubmitted()
        {
            await ClickingContinueNavigatesBackToReviewwPage().ConfigureAwait(false);

            Click("Continue");
            await AxeHelper.CheckAccessibilityAsync(this).ConfigureAwait(false);
            ExpectHeader("Review 2020 group report for " + Submission.OrgName_Blackpool);

            //both pages complete now

            ExpectNo("Submission incomplete. Section 1 must be completed in order to submit.");

            AtRow("Your modern slavery statment").Expect("Completed");
            AtRow("Areas covered by your modern slavery statement").Expect("Completed");

            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}