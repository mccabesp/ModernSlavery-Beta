using Geeks.Pangolin;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Testing.Helpers.Extensions;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;

namespace ModernSlavery.Hosts.Web.Tests
{
    [TestFixture]
    public class Second_User_Cannot_Edit_Draft : Create_Account_Add_Second_User
    {
        const string _firstname = Create_Account.roger_first; const string _lastname = Create_Account.roger_last; const string _title = Create_Account.roger_job_title; const string _email = Create_Account.roger_email; const string _password = Create_Account.roger_password;


        private Organisation org;
        [OneTimeSetUp]
        public async Task SetUp()
        {
            //HostHelper.ResetDbScope();



        }


        [Test, Order(29)]
        public async Task RegisterOrgs()
        {
            org = this.Find<Organisation>(org => org.GetLatestActiveScope().ScopeStatus.IsAny(ScopeStatuses.PresumedOutOfScope, ScopeStatuses.PresumedInScope) && org.LatestRegistrationUserId == null && !org.UserOrganisations.Any());

            await this.RegisterUserOrganisationAsync(org.OrganisationName, UniqueEmail);
            await this.RegisterUserOrganisationAsync(org.OrganisationName, Create_Account.second_email);
            RefreshPage();

            await this.SaveDatabaseAsync();

            await Task.CompletedTask;
        }
        [Test, Order(40)]
        public async Task StartSubmission()
        {

            ExpectHeader("Select an organisation");

            Click(org.OrganisationName);
            SubmissionHelper.MoreInformationRequiredComplete(this, true, OrgName: org.OrganisationName);
            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST");
            ExpectHeader(That.Contains, "Manage your modern slavery statement submissions");


            Click(The.Top, "Start Draft");
            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST");
            ExpectHeader("Before you start");
            Click("Start Now");
            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST");
            ModernSlavery.Testing.Helpers.Extensions.SubmissionHelper.GroupOrSingleScreenComplete(this, OrgName: TestData.OrgName);

            await Task.CompletedTask;
        }

        [Test, Order(42)]
        public async Task YourModernSlaveryStatement()
        {

            ExpectHeader("Your modern slavery statement");

            Set("URL").To(Submission.YourMSStatement_URL);

            Submission_Helper.DateSet(this, Submission.YourMSStatement_To_Day, Submission.YourMSStatement_To_Month, Submission.YourMSStatement_From_Year, "1");
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
        public async Task SignOutAndContinueDraftAsFirstUser()
        {
            Click("Sign out");

            Click(The.Top, "Sign in");

            ExpectHeader("Sign in or create an account");
            Set("Email").To(UniqueEmail);
            Set("Password").To(_password);

            Click(The.Bottom, "Sign in");

            ExpectHeader("Select an organisation");

            Click(org.OrganisationName);
            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST");
            ExpectHeader(That.Contains, "Manage your modern slavery statement submissions");


            Click(The.Top, "Continue");

            await Task.CompletedTask;
        }

        [Test, Order(46)]
        public async Task ExpectStatementLockedText()
        {
            ExpectHeader("Statement Summary Locked");

            Expect("Another user is currently editing this statement summary. Please try again later");


            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST");
            await Task.CompletedTask;
        }

        [Test, Order(48)]
        public async Task ClickingContinueLeadsToSubmissionsPage()
        {
            Click("Continue");
            ExpectHeader(That.Contains, "Manage your modern slavery statement submissions");

            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST");
            await Task.CompletedTask;
        }
    }
}
