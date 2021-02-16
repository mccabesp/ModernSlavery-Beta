﻿using Geeks.Pangolin;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Testing.Helpers.Extensions;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;

namespace ModernSlavery.Hosts.Web.Tests
{
    [TestFixture]


    public class Submission_PoliciesContentCheck : CreateAccount
    {
        const string _firstname = Create_Account.roger_first; const string _lastname = Create_Account.roger_last; const string _title = Create_Account.roger_job_title; const string _email = Create_Account.roger_email; const string _password = Create_Account.roger_password;


        private Organisation org;
        [OneTimeSetUp]
        public async Task SetUp()
        {
        }

        public Submission_PoliciesContentCheck() : base(_firstname, _lastname, _title, _email, _password)
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
        public async Task NavigateToPolicies()
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

            Click("Policies");
            ExpectHeader("Do your organisation's policies include any of the following provisions in relation to modern slavery?");

            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(44)]
        public async Task Policies()
        {

            Try(
                 () => ExpectHeader("Do your organisation's policies include any of the following provisions in relation to modern slavery?"),
            () => { ExpectLabel("Freedom of workers to terminate employment"); },
            () => { ExpectLabel("Freedom of movement"); },
            () => { ExpectLabel("Freedom of association"); },
            () => { ExpectLabel("Prohibits any threat of violence, harassment and intimidation"); },
            () => { ExpectLabel("Prohibits the use of worker-paid recruitment fees"); },
            () => { ExpectLabel("Prohibits compulsory overtime"); },
            () => { ExpectLabel("Prohibits child labour"); },
            () => { ExpectLabel("Prohibits discrimination"); },
            () => { ExpectLabel("Prohibits confiscation of workers original identification documents"); },
            () => { ExpectLabel("Provides access to remedy, compensation and justice for victims of modern slavery"); },
            () => { ExpectLabel("Other"); },
            () => { Below("Or").ExpectLabel("Your organisation's policies do not include any of these provisions in relation to modern slavery"); },
            () => { ExpectButton("Save and continue"); },
            () => { ExpectLink("Skip this question"); });


            ClickLabel("Other");
            ExpectLabel("Please specify");
            ClickLabel("Other");
            ExpectNoLabel("Please specify");

            Click("Skip this question");

            ExpectHeader("Add your 2020 modern slavery statement to the registry");
            await Task.CompletedTask.ConfigureAwait(false);

        }

    }
}