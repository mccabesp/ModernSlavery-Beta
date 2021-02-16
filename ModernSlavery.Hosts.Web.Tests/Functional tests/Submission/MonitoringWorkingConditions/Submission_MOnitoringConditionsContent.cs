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
    public class Submission_MonitoringConditionsContent : CreateAccount
    {
        const string _firstname = Create_Account.roger_first; const string _lastname = Create_Account.roger_last; const string _title = Create_Account.roger_job_title; const string _email = Create_Account.roger_email; const string _password = Create_Account.roger_password;

        private Organisation org;
        [OneTimeSetUp]
        public async Task SetUp()
        {
        }

        public Submission_MonitoringConditionsContent() : base(_firstname, _lastname, _title, _email, _password)
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
        public async Task NavigateToReccomendedMonitoringContent()
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

            Click("Monitoring working conditions");
            ExpectHeader("During the period of the statement, who did you engage with to help you monitor working conditions across your operations and supply chain?");

            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(44)]
        public async Task StatementUrl()
        {
            Try(
                () => ExpectHeader("During the period of the statement, who did you engage with to help you monitor working conditions across your operations and supply chain?"),
                () => { ExpectLabel("Your suppliers"); },
                () => { ExpectLabel("Trade unions or worker representative groups"); },
                () => { ExpectLabel("Civil society organisations"); },
                () => { ExpectLabel("Professional auditors"); },
                () => { ExpectLabel("Workers within your organisation"); },
                () => { ExpectLabel("Workers within your supply chain"); },
                () => { ExpectLabel("Central or local government"); },
                () => { ExpectLabel("Law enforcement, such as police, GLAA and other local labour market inspectorates"); },
                () => { ExpectLabel("Businesses in your industry or sector"); },
                () => { Below("Or").ExpectLabel("Your organisation did not engage with others to help monitor working conditions across your operations and supply chain"); },
                () => { ExpectButton("Save and continue"); },
                () => { ExpectLink("Skip this question"); });

            Click("Skip this question");

            ExpectHeader("Did you use social audits to look for signs of forced labour?");

            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(46)]
        public async Task SocialAudits()
        {
            Try(
                () => ExpectHeader("Did you use social audits to look for signs of forced labour?"),
                () => { ExpectLabel("Audit conducted by your staff"); },
                () => { ExpectLabel("Third party audit arranged by your organisation"); },
                () => { ExpectLabel("Audit conducted by your supplier’s staff"); },
                () => { ExpectLabel("Third party audit arranged by your supplier"); },
                () => { ExpectLabel("Announced audit"); },
                () => { ExpectLabel("Unannounced audit"); },
                () => { Below("Or").ExpectLabel("Your organisation did not carry out any social audits during the period of the statement"); },
                () => { ExpectButton("Save and continue"); },
                () => { ExpectLink("Skip this question"); });


            ClickText("What are social audits?");
            Expect(What.Contains, "A social audit is a review of an organisation’s working practices from the point of view of social responsibility, and should include an evaluation of working conditions in the organisation’s operations and supply chains.");
            Expect(What.Contains, "By their nature, audits of supplier workplaces represent a snapshot in time.");


            ClickText("What are social audits?");
            ExpectNo(What.Contains, "A social audit is a review of an organisation's working practices from the point of view of social responsibility, and should include an evaluation of working conditions in the organisation’s operations and supply chains.");
            ExpectNo(What.Contains, "By their nature, audits of supplier workplaces represent a snapshot in time.");

            Click("Skip this question");

            ExpectHeader("What types of grievance mechanisms did you have in place?");

            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(48)]
        public async Task GrievanceMechanisms()
        {
            Try(
                () => ExpectHeader("What types of grievance mechanisms did you have in place?"),
                () => { Expect("Tell us how workers in your operations or supply chains could raise concerns or make complaints."); },
                () => { Expect("Select all that apply"); },
                () => { ExpectLabel("Using anonymous whistleblowing services, such as a helpline or mobile phone app"); },
                () => { ExpectLabel("Through trade unions or other worker representative groups"); },
                () => { Below("Or").ExpectLabel("There were no processes in your operations or supply chains for workers to raise concerns or make complaints"); },
                () => { ExpectButton("Save and continue"); },
                () => { ExpectLink("Skip this question"); });



            Click("Skip this question");

            ExpectHeader("Are there any other ways you monitored working conditions across your operations and supply chains?");

            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(50)]
        public async Task OtherWays()
        {
            Try(
                () => ExpectHeader("Are there any other ways you monitored working conditions across your operations and supply chains?"),
                () => { Expect("Tell us briefly what you did"); },
                () => { Expect("You have 200 characters remaining"); },
                () => { ExpectXPath("//textarea"); },
                () => { ExpectButton("Save and continue"); },
                () => { ExpectLink("Skip this question"); });

            Click("Skip this question");

            ExpectHeader("Add your 2020 modern slavery statement to the registry");

            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}