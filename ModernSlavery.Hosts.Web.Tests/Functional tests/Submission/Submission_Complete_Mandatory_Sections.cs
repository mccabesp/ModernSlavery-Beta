using Geeks.Pangolin;
using Microsoft.Azure.Management.CosmosDB.Fluent.Models;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Testing.Helpers.Extensions;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;

namespace ModernSlavery.Hosts.Web.Tests
{
    [TestFixture]


    public class Submission_Complete_Mandatory_Sections : Private_Registration_Success
    {
        //protected Organisation org;
        //[OneTimeSetUp]
        //public async Task SetUp()
        //{
        //    //HostHelper.ResetDbScope();
        //    org = this.Find<Organisation>(org => org.GetLatestActiveScope().ScopeStatus.IsAny(ScopeStatuses.PresumedOutOfScope, ScopeStatuses.PresumedInScope) && org.LatestRegistrationUserId == null && !org.UserOrganisations.Any());

        //}

        [Test, Order(40)]
        public async Task StartSubmission()
        {

            ExpectHeader("Select an organisation");

            Click(org.OrganisationName);
            await AxeHelper.CheckAccessibilityAsync(this);
            SubmissionHelper.MoreInformationRequiredComplete(this, true, OrgName: org.OrganisationName);



            Click(The.Top, "Start Draft");
            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST");
            ExpectHeader("Before you start");
            Click("Start Now");
            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST");
            ModernSlavery.Testing.Helpers.Extensions.SubmissionHelper.GroupOrSingleScreenComplete(this, OrgName: org.OrganisationName);

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
            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST");
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
            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST");
            ExpectHeader(That.Contains, "Your organisation");

            await Task.CompletedTask;
        }

        [Test, Order(46)]
        public async Task NavigatePastOptionalSections()
        {
            
            Click("Continue");
            await AxeHelper.CheckAccessibilityAsync(this);
            ExpectHeader("Policies");

            Click("Continue");
            await AxeHelper.CheckAccessibilityAsync(this);
            ExpectHeader(That.Contains, "Supply chain risks and due diligence");

            Click("Continue");
            await AxeHelper.CheckAccessibilityAsync(this);
            ExpectHeader(That.Contains, "Supply chain risks and due diligence");

            Click("Continue");
            await AxeHelper.CheckAccessibilityAsync(this);
            ExpectHeader("Training");

            Click("Continue");
            await AxeHelper.CheckAccessibilityAsync(this);
            ExpectHeader("Monitoring progress");

            Click("Continue");
            await AxeHelper.CheckAccessibilityAsync(this);
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
            ExpectButton("Submit for publication");
            ExpectButton("Exit and save Changes");
            ExpectButton("Exit and lose Changes");
            await Task.CompletedTask;
        }
          [Test, Order(49)]
        public async Task ExitAndSaveChanges()
        {  
        Click("Exit and save Changes");
            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST");
            ExpectHeader(That.Contains, "Manage your modern slavery statement submissions");

            await Task.CompletedTask;
        }



    }
}