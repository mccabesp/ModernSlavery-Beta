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


    public class Submission_SectorsAndTurnoverContentCheck : CreateAccount
    {
        const string _firstname = Create_Account.roger_first; const string _lastname = Create_Account.roger_last; const string _title = Create_Account.roger_job_title; const string _email = Create_Account.roger_email; const string _password = Create_Account.roger_password;


        private Organisation org;
        [OneTimeSetUp]
        public async Task SetUp()
        {
        }

        public Submission_SectorsAndTurnoverContentCheck() : base(_firstname, _lastname, _title, _email, _password)
        {
        }

        [Test, Order(29)]
        public async Task RegisterOrg()
        {
            org = this.Find<Organisation>(org => org.GetLatestActiveScope().ScopeStatus.IsAny(ScopeStatuses.PresumedOutOfScope, ScopeStatuses.PresumedInScope) && org.LatestRegistrationUserId == null && !org.UserOrganisations.Any());

            await this.RegisterUserOrganisationAsync(org.OrganisationName, UniqueEmail).ConfigureAwait(false);
            RefreshPage();

            await this.SaveDatabaseAsync().ConfigureAwait(false);

            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(41)]
        public async Task NavigateToSectorsAndTurnover()
        {
            ExpectHeader("Register or select organisations you want to add statements for");

            Click(org.OrganisationName);
            SubmissionHelper.MoreInformationRequiredComplete(this, true, OrgName: org.OrganisationName);
            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST").ConfigureAwait(false);
            ExpectHeader(That.Contains, "Manage your modern slavery statements");


            Click(The.Top, "Start Draft");
            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST").ConfigureAwait(false);
            ExpectHeader("Before you start");


            Click("Continue");
            await AxeHelper.CheckAccessibilityAsync(this).ConfigureAwait(false);
            ExpectHeader("Transparency and modern slavery");

            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST").ConfigureAwait(false);

            Click("Continue");
            await AxeHelper.CheckAccessibilityAsync(this).ConfigureAwait(false);
            ExpectHeader("Add your 2020 modern slavery statement to the registry");


            Click("Organisations covered by the statement");
            ModernSlavery.Testing.Helpers.Extensions.SubmissionHelper.GroupOrSingleScreenComplete(this, OrgName: TestData.OrgName);

            Click("Your organisation's sectors and turnover");
            ExpectHeader("Which sectors does your organisation operate in?");

            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(44)]
        public async Task Sectors()
        {

            Try(
                 () => ExpectHeader("Which sectors does your organisation operate in?"),
            () => { Expect("If your statement is for a group, select sectors for all organisations in the group."); },
            () => { Expect("Select all that apply"); },
            () => { ExpectLabel("Accommodation and food service activities"); },
            () => { ExpectLabel("Activities of extraterritorial organisations and bodies"); },
            () => { ExpectLabel("Activities of households as employers"); },
            () => { ExpectLabel("Administrative and support service activities"); },
            () => { ExpectLabel("Agriculture, Forestry and Fishing"); },
            () => { ExpectLabel("Arts, entertainment and recreation"); },
            () => { ExpectLabel("Construction"); },
            () => { ExpectLabel("Education"); },
            () => { ExpectLabel("Electricity, gas, steam and air conditioning supply"); },
            () => { ExpectLabel("Financial and insurance activities"); },
            () => { ExpectLabel("Human health and social work activities"); },
            () => { ExpectLabel("Information and communication"); },
            () => { ExpectLabel("Manufacturing"); },
            () => { ExpectLabel("Mining and Quarrying"); },
            () => { ExpectLabel("Other service activities"); },
            () => { ExpectLabel("Professional scientific and technical activities"); },
            () => { ExpectLabel("Public administration and defence"); },
            () => { ExpectLabel("Public sector"); },
            () => { ExpectLabel("Real estate activities"); },
            () => { ExpectLabel("Transportation and storage"); },
            () => { ExpectLabel("Water supply, sewerage, waste management and remediation activities"); },
            () => { ExpectLabel("Wholesale and retail trade"); },
            () => { ExpectLabel("Other"); },
            () => { ExpectButton("Save and continue"); },
            () => { ExpectLink("Skip this question"); });

            Click("Skip this question");

            ExpectHeader("What was your turnover during the financial year the statement relates to?");
            await Task.CompletedTask.ConfigureAwait(false);

        }
        [Test, Order(46)]
        public async Task Turnover()
        {

            Try(
                 () => ExpectHeader("What was your turnover during the financial year the statement relates to?"),
            () => { Expect("If your statement is for a group, include the total turnover for all the organisations in the group."); },
            () => { Expect("If you’re a public body, base your answer on your organisation’s budget."); },
            () => { ExpectLabel("Under £36 million"); },
            () => { ExpectLabel("£36 million to £60 million"); },
            () => { ExpectLabel("£60 million to £100 million"); },
            () => { ExpectLabel("£100 million to £500 million"); },
            () => { ExpectLabel("Over £500 million"); },
            () => { ExpectButton("Save and continue"); },
            () => { ExpectLink("Skip this question"); });


            //read guidance
            ClickText("Read guidance on how to calculate turnover");
            Expect(What.Contains, "Advice on calculating your turnover or budget, including for charities and investment trusts, is available in the government’s");
            ExpectLink(That.Contains, "guidance on publishing a statement (opens in new window)");
            Click("Skip this question");

            ExpectHeader("Add your 2020 modern slavery statement to the registry");
            await Task.CompletedTask.ConfigureAwait(false);

        }

    }
}