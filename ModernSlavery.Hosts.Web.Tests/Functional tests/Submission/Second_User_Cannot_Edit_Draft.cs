using Geeks.Pangolin;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Testing.Helpers.Extensions;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;

namespace ModernSlavery.Hosts.Web.Tests
{
    [TestFixture, Ignore("Awaiting Submission merge")]
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
        public async Task RegisterOrg()
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
    }
}
