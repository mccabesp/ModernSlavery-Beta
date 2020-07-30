using Geeks.Pangolin;
using NUnit.Framework;
using System.Threading.Tasks;

namespace ModernSlavery.Hosts.Web.Tests
{
    [TestFixture, Ignore("Awaiting Submission merge")]

    public class Submission_Complete_Mandatory_Sections : Private_Registration_Success
    {
        [Test, Order(40)]
        public async Task StartSubmission()
        {

            ExpectHeader("Select an organisation");

            Click(Submission.OrgName_Blackpool);

            ExpectHeader("Manage your organisations reporting");

            AtRow("2019/20").Click("Draft report");

            ExpectHeader("Before you start");
            Click("Start Now");

            ExpectHeader("Your modern slavery statement");
            await Task.CompletedTask;
        }

        [Test, Order(42)]
        public async Task CompleteYourModernSlaverySection()
        {

            Set("URL").To(Submission.YourMSStatement_URL);

            Submission_Helper.DateSet(this, Submission.YourMSStatement_To_Day, Submission.YourMSStatement_To_Month, Submission.YourMSStatement_To_Year, "1");
            Submission_Helper.DateSet(this, Submission.YourMSStatement_To_Day, Submission.YourMSStatement_To_Month, Submission.YourMSStatement_To_Year, "2");

            Set("First name").To(Submission.YourMSStatement_First);
            Set("Last name").To(Submission.YourMSStatement_Last);
            Set("Job title").To(Submission.YourMSStatement_JobTitle);

            Submission_Helper.DateSet(this, Submission.YourMSStatement_ApprovalDate_Day, Submission.YourMSStatement_ApprovalDate_Month, Submission.YourMSStatement_ApprovalDate_Year, "3");

            Click("Continue");

            await Task.CompletedTask;
        }

        [Test, Order(44)]
        public async Task CompleteAreasCoveredSection()
        {
            ExpectHeader("Areas covered by your modern slavery statement");

            AtLabel("Your organisation’s structure, business and supply chains").ClickLabel("Yes");
            AtLabel("Policies").ClickLabel("Yes");
            AtLabel("Risk assessment and management").ClickLabel("Yes");
            AtLabel("Due diligence processes").ClickLabel("Yes");
            AtLabel("Staff training about slavery and human trafficking").ClickLabel("Yes");
            AtLabel("Goals and key performance indicators (KPIs) to measure your progress over time, and the effectiveness of your actions").ClickLabel("Yes");

            Click("Continue");

            ExpectHeader("Supply Chain Risks and due diligence");


            await Task.CompletedTask;
        }

    }
}