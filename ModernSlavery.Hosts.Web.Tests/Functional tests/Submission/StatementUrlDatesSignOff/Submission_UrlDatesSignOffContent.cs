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
    public class Submission_UrlDatesSignOffContent : CreateAccount
    {
        const string _firstname = Create_Account.roger_first; const string _lastname = Create_Account.roger_last; const string _title = Create_Account.roger_job_title; const string _email = Create_Account.roger_email; const string _password = Create_Account.roger_password;

        private Organisation org;
        [OneTimeSetUp]
        public async Task SetUp()
        {
        }

        public Submission_UrlDatesSignOffContent() : base(_firstname, _lastname, _title, _email, _password)
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
        public async Task NavigateToUrlDatesSignOff()
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

            Click("Statement URL, dates and sign-off");
            ExpectHeader("Provide a link to the modern slavery statement on your organisation's website");

            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(44)]
        public async Task StatementUrl()
        {
            Try(
                () => ExpectHeader("Provide a link to the modern slavery statement on your organisation's website"),
                () => { Expect("URL"); },
                () => { Expect("URL must begin with ‘https://' or ‘http://'"); },
                () => { ExpectField("URL"); },
                () => { Expect("If you do not have a website"); },
                () => { ExpectButton("Save and continue"); },
                () => { ExpectLink("Skip this question"); });

            //If You do not have
            ClickText("If you do not have a website");
            Try(
                () => ExpectHeader("Provide a link to the modern slavery statement on your organisation's website"),
                () => { ExpectField("Provide an email address"); },
                () => { Expect("If you do not have a website, provide an email address that can be used to request a copy of the statement. It will be published on GOV.UK as part of your statement summary on the registry."); },
                () => { Expect(What.Contains, "On request, provide a copy within 30 days"); },
                () => { Expect(What.Contains, "If you have to publish a modern slavery statement by law and you don’t have a website, you must provide a copy of the statement in writing to anyone who requests one within 30 days."); },
                () => { Expect(What.Contains, "Organisations in a group"); },
                () => { Expect(What.Contains, "If your organisation is part of a group, we would encourage you to publish your statement on the website of your parent organisation, and add that URL to our online service."); });

            Click("Skip this question");

            ExpectHeader("What period does this statement cover?");
            await Task.CompletedTask.ConfigureAwait(false);

        }

        [Test, Order(45)]
        public async Task Period()
        {
            Try(
                () => ExpectHeader("What period does this statement cover?"),
                () => { Expect("For example, 31 3 1980"); },
                () => { RightOf("to").ExpectField("Day"); },
                () => { RightOf("to").ExpectField("Month"); },
                () => { RightOf("to").ExpectField("Year"); },
                () => { LeftOf("to").ExpectField("Day"); },
                () => { LeftOf("to").ExpectField("Month"); },
                () => { LeftOf("to").ExpectField("Year"); },
                () => { ExpectButton("Save and continue"); },
                () => { ExpectLink("Skip this question"); });

            Click("Skip this question");

            ExpectHeader("What is the name of the director (or equivalent) who signed off your statement?");

            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(46)]
        public async Task Director()
        {
            Try(
                () => ExpectHeader("What is the name of the director (or equivalent) who signed off your statement?"),
                () => { ExpectField("First name"); },
                () => { ExpectField("Last name"); },
                () => { ExpectField("Job Title"); },
                () => { ExpectField("Day"); },
                () => { ExpectField("Month"); },
                () => { ExpectField("Year"); },
                () => { Expect("What date was your statement approved by the board or equivalent management body?"); },
                () => { Expect("For example, 31 3 1980"); },
                () => { ExpectButton("Save and continue"); },
                () => { ExpectLink("Skip this question"); });

            Click("Skip this question");

            ExpectHeader("Add your 2020 modern slavery statement to the registry");
            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}