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


    public class Submission_ReccomendedAreasContent : CreateAccount
    {
        const string _firstname = Create_Account.roger_first; const string _lastname = Create_Account.roger_last; const string _title = Create_Account.roger_job_title; const string _email = Create_Account.roger_email; const string _password = Create_Account.roger_password;


        private Organisation org;
        [OneTimeSetUp]
        public async Task SetUp()
        {
        }

        public Submission_ReccomendedAreasContent() : base(_firstname, _lastname, _title, _email, _password)
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
        public async Task NavigateToReccomendedAreasContent()
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

            Click("Recommended areas covered by the statement");
            ExpectHeader("Does your statement cover the following areas in relation to modern slavery?");

            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(44)]
        public async Task StatementUrl()
        {

            Try(
                 () => ExpectHeader("Does your statement cover the following areas in relation to modern slavery?"),
            () => { BelowHeader("Your organisation’s structure, business and supply chains").AboveHeader("Policies").ExpectLabel("Yes"); },
            () => { BelowHeader("Your organisation’s structure, business and supply chains").AboveHeader("Policies").ExpectLabel("No"); },
            () => { BelowHeader("Policies").AboveHeader("Risk assessment").ExpectLabel("Yes"); },
            () => { BelowHeader("Policies").AboveHeader("Risk assessment").ExpectLabel("No"); },
            () => { BelowHeader("Risk assessment").AboveHeader("Due diligence (steps to address risk)").ExpectLabel("Yes"); },
            () => { BelowHeader("Risk assessment").AboveHeader("Due diligence (steps to address risk)").ExpectLabel("No"); },
            () => { BelowHeader("Due diligence (steps to address risk)").AboveHeader("Training about modern slavery").ExpectLabel("Yes"); },
            () => { BelowHeader("Due diligence (steps to address risk)").AboveHeader("Training about modern slavery").ExpectLabel("No"); },
            () => { BelowHeader("Training about modern slavery").AboveHeader("Goals and key performance indicators (KPIs) to measure the effectiveness of your actions and progress over time").ExpectLabel("Yes"); },
            () => { BelowHeader("Training about modern slavery").AboveHeader("Goals and key performance indicators (KPIs) to measure the effectiveness of your actions and progress over time").ExpectLabel("No"); },
            () => { BelowHeader("Goals and key performance indicators (KPIs) to measure the effectiveness of your actions and progress over time").ExpectLabel("No"); },
            () => { BelowHeader("Goals and key performance indicators (KPIs) to measure the effectiveness of your actions and progress over time").ExpectLabel("No"); },
            () => { ExpectButton("Save and continue"); },
            () => { ExpectLink("Skip this question"); });

            Click("Skip this question");

            ExpectHeader("Add your 2020 modern slavery statement to the registry");
            await Task.CompletedTask.ConfigureAwait(false);

        }


    }
}