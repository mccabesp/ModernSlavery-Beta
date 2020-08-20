using Geeks.Pangolin;
using NUnit.Framework;
using System.Threading.Tasks;

namespace ModernSlavery.Hosts.Web.Tests
{
    [TestFixture]

    public class Submission_Complete_Mandatory_Sections : Private_Registration_Success
    {
        [Test, Order(40)]
        public async Task StartSubmission()
        {

            ExpectHeader("Select an organisation");

            Click(Submission.OrgName_InterFloor);

            ExpectHeader(That.Contains, "Manage your modern slavery statement submissions");


            Click("Start Draft");

            ExpectHeader("Before you start");
            Click("Start Now");

            await Task.CompletedTask;
        }

        [Test, Order(42)]
        public async Task YourModernSlaveryStatement()
        {

            ExpectHeader("Your modern slavery statement");

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
        public async Task AreasCoveredByYourModernSlaveryStatement()
        {
            ExpectHeader("Areas covered by your modern slavery statement");

            BelowHeader("Your organisation’s structure, business and supply chains").ClickLabel(The.Top, "Yes");
            BelowHeader("Policies").ClickLabel(The.Top, "Yes");
            BelowHeader("Risk assessment and management").ClickLabel(The.Top, "Yes");
            BelowHeader("Due diligence processes").ClickLabel(The.Top, "Yes");
            BelowHeader("Staff training about slavery and human trafficking").ClickLabel(The.Top, "Yes");
            BelowHeader("Goals and key performance indicators (KPIs) to measure your progress over time, and the effectiveness of your actions").ClickLabel(The.Top, "Yes");

            Click("Continue");
            ExpectHeader(That.Contains, "Your organisation");

            await Task.CompletedTask;
        }

        [Test, Order(46)]
        public async Task NavigatePastOptionalSections()
        {
            
            Click("Continue");
            ExpectHeader("Policies");

            Click("Continue");
            ExpectHeader(That.Contains, "Supply chain risks and due diligence");

            Click("Continue");
            ExpectHeader(That.Contains, "Supply chain risks and due diligence");

            Click("Continue");
            ExpectHeader("Training");

            Click("Continue");
            ExpectHeader("Monitoring progress");

            Click("Continue");
            ExpectHeader("Review before submitting");

            await Task.CompletedTask;
        }

        [Test, Order(47)]
        public async Task OptionalSectionsIncomplete()
        {
            //mandaotry sections should be completed
            RightOf("Your modern Slavery statement").Expect("Completed");
            RightOf("Areas covered by your modern statement").Expect("Completed");

            //optional sections incomplete
            RightOf("Your organisation").Expect("Not Completed");
            RightOf("Policies").Expect("Not Completed");
            RightOf("Supply chain risks and due diligence (part 1)").Expect("Not Completed");
            RightOf("Supply chain risks and due diligence (part 2)").Expect("Not Completed");
            RightOf("Training").Expect("Not Completed");
            RightOf("Monitoring progress").Expect("Not Completed");
            await Task.CompletedTask;
        }

        [Test, Order(48)]
        public async Task ExpectButtons()
        {
            ExpectButton("Confirm and submit");
            ExpectButton("Exit and save Changes");
            ExpectButton("Exit and lose Changes");
            await Task.CompletedTask;
        }
          [Test, Order(49)]
        public async Task ExitAndSaveChanges()
        {  
        Click("Exit and save Changes");

            ExpectHeader(That.Contains, "Manage your modern slavery statement submissions");

            await Task.CompletedTask;
        }



    }
}