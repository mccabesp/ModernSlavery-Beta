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


    public class Submission_TrainingContentCheck : CreateAccount
    {
        const string _firstname = Create_Account.roger_first; const string _lastname = Create_Account.roger_last; const string _title = Create_Account.roger_job_title; const string _email = Create_Account.roger_email; const string _password = Create_Account.roger_password;


        private Organisation org;
        [OneTimeSetUp]
        public async Task SetUp()
        {
        }

        public Submission_TrainingContentCheck() : base(_firstname, _lastname, _title, _email, _password)
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
        public async Task NavigateToTraining()
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

            Click("Training");
            ExpectHeader("If you provided training on modern slavery during the period of the statement, who was it for?");

            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(44)]
        public async Task Training()
        {
            ClickText("What kind of training is relevant");
            Expect(What.Contains, "Anything designed to increase knowledge and skills around identifying, addressing or preventing modern slavery risks is relevant.");
            Expect(What.Contains, "Training can take a range of forms, from formal training courses to broader awareness-raising activities such as workshops or webinars.");

            Expect(What.Contains, "You can read more about training in the");

            ExpectLink(That.Contains, "statutory guidance (opens in new window)");

            ClickText("What kind of training is relevant");

            ExpectNo(What.Contains, "Anything designed to increase knowledge and skills around identifying, addressing or preventing modern slavery risks is relevant.");
            ExpectNo(What.Contains, "Training can take a range of forms, from formal training courses to broader awareness-raising activities such as workshops or webinars.");

            ExpectNo(What.Contains, "You can read more about training in the");

            ExpectNoLink(That.Contains, "statutory guidance (opens in new window)");


            Try(
                 () => ExpectHeader("If you provided training on modern slavery during the period of the statement, who was it for?"),
            () => { ExpectLabel("Your whole organisation"); },
            () => { ExpectLabel("Your front line staff"); },
            () => { ExpectLabel("Human resources"); },
            () => { ExpectLabel("Executive-level staff"); },
            () => { ExpectLabel("Procurement staff"); },
            () => { ExpectLabel("Your suppliers"); },
            () => { ExpectLabel("The wider community"); },
            () => { ExpectLabel("Other"); },
            () => { Below("Or").ExpectLabel("Your organisation did not provide training on modern slavery during the period of the statement"); },
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