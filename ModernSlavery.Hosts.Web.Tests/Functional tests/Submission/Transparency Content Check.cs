using Geeks.Pangolin;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Testing.Helpers.Extensions;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;

namespace ModernSlavery.Hosts.Web.Tests
{

    public class Transparency_And_Modern_Slavery_Content_Check : CreateAccount
    {
        const string _firstname = Create_Account.roger_first; const string _lastname = Create_Account.roger_last; const string _title = Create_Account.roger_job_title; const string _email = Create_Account.roger_email; const string _password = Create_Account.roger_password;

        private Organisation org;
        [OneTimeSetUp]
        public async Task SetUp()
        {
            org = this.Find<Organisation>(org => org.GetLatestActiveScope().ScopeStatus.IsAny(ScopeStatuses.PresumedOutOfScope, ScopeStatuses.PresumedInScope) && org.LatestRegistrationUserId == null && !org.UserOrganisations.Any());
            await Task.CompletedTask.ConfigureAwait(false);
        }

        public Transparency_And_Modern_Slavery_Content_Check() : base(_firstname, _lastname, _title, _email, _password)
        {


        }

        [Test, Order(29)]
        public async Task RegisterOrg()
        {
            await this.RegisterUserOrganisationAsync(org.OrganisationName, UniqueEmail).ConfigureAwait(false);
        }

        [Test, Order(40)]
        public async Task NavigateToTransparency()
        {

            RefreshPage();

            ExpectHeader("Register or select organisations you want to add statements for");

            Click(org.OrganisationName);
            await AxeHelper.CheckAccessibilityAsync(this).ConfigureAwait(false);
            SubmissionHelper.MoreInformationRequiredComplete(this, true, OrgName: org.OrganisationName);

            ExpectHeader(That.Contains, "Manage your modern slavery statements");

            Click(The.Bottom, "Start Draft");

            Click("Continue");
            await AxeHelper.CheckAccessibilityAsync(this).ConfigureAwait(false);

            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(41)]
        public async Task PageHeaderContent()
        {
            ExpectHeader("Transparency and modern slavery");

            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(42)]
        public async Task ContentCheck()
        {
            Try(
                () => { Expect("Under section 54 of the Modern Slavery Act 2015 modern slavery statements must cover the steps that all relevant organisations have taken to ensure that slavery and human trafficking is not taking place in any of their supply chains or parts of their business."); },
                () => { Expect("The purpose of the Modern slavery statement registry is to increase supply chain transparency and improve understanding of modern slavery risks and best practice."); },
                () => { ExpectText(That.Contains, "We acknowledge that identifying and addressing modern slavery risks is a long-term challenge. Our aim is for organisations to:"); },
                () => { ExpectText(That.Contains, "be open and transparent about modern slavery risks in their operations and supply chains"); },
                () => { ExpectText(That.Contains, "target their actions and prioritise risks in order to have the most impact"); },
                () => { ExpectText(That.Contains, "make progress over time to address those risks and improve outcomes for workers"); },
                () => { Expect("We encourage you to answer all questions as fully as possible."); });

            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(44)]
        public async Task BackButtonCheck()
        {
            Click("Back");
            ExpectHeader("Before you start");

            //return to transparency

            Click("Continue");
            ExpectHeader("Transparency and modern slavery");

            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(46)]
        public async Task ExpectAndUseContinue()
        {
            Expect("Continue");
            Click("Continue");
            await AxeHelper.CheckAccessibilityAsync(this).ConfigureAwait(false);
            ExpectHeader("Add your 2019 modern slavery statement to the registry");

            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}