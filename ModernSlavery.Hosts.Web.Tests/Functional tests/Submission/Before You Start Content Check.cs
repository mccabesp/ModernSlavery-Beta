using Geeks.Pangolin;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Testing.Helpers.Extensions;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;

namespace ModernSlavery.Hosts.Web.Tests
{

    public class Before_You_Start_Content_Check : CreateAccount
    {
        const string _firstname = Create_Account.roger_first; const string _lastname = Create_Account.roger_last; const string _title = Create_Account.roger_job_title; const string _email = Create_Account.roger_email; const string _password = Create_Account.roger_password;


        private Organisation org;
        [OneTimeSetUp]
        public async Task SetUp()
        {
            org = this.Find<Organisation>(org => org.GetLatestActiveScope().ScopeStatus.IsAny(ScopeStatuses.PresumedOutOfScope, ScopeStatuses.PresumedInScope) && org.LatestRegistrationUserId == null && !org.UserOrganisations.Any());
            await Task.CompletedTask.ConfigureAwait(false);
        }

        public Before_You_Start_Content_Check() : base(_firstname, _lastname, _title, _email, _password)
        {


        }

        [Test, Order(29)]
        public async Task RegisterOrg()
        {
            await this.RegisterUserOrganisationAsync(org.OrganisationName, UniqueEmail).ConfigureAwait(false);
        }


        [Test, Order(34)]
        public async Task NavigateToBeforeYouStart()
        {

            RefreshPage();

            ExpectHeader("Register or select organisations you want to add statements for");

            Click(org.OrganisationName);
            await AxeHelper.CheckAccessibilityAsync(this).ConfigureAwait(false);
            SubmissionHelper.MoreInformationRequiredComplete(this, true, OrgName: org.OrganisationName);

            ExpectHeader(That.Contains, "Manage your modern slavery statements");

            Click(The.Bottom, "Start Draft");


            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(36)]
        public async Task PageHeaderContent()
        {
            ExpectHeader("Before you start");
            Expect("To use this service, you will need to provide us with some basic information about your most recent modern slavery statement.");
            Expect("We will then ask you some additional questions about your statement. These are optional – but we strongly encourage you to complete them. All our questions relate to the period covered by your statement.");

            await Task.CompletedTask.ConfigureAwait(false);
        }
        [Test, Order(38)]
        public async Task FirstParagraphCheck()
        {
            ExpectHeader("Saving and editing your answers");
            Expect("You do not have to answer all our questions in one go. You can save and edit your answers as often as you like before you submit them.");
            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(42)]
        public async Task SecondParagraphCheck()
        {
            ExpectHeader("Publishing your answers on GOV.UK");
            Expect("When you submit your answers, we will publish all the information you’ve provided as a statement summary on the Modern slavery statement registry on GOV.UK. This will include a link to the full statement on your website. It will be available for public viewing.");
            Expect("If you need to, you can make changes to your published answers and resubmit them. Your updated information will then replace your original answers on the registry.");
            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(44)]
        public async Task ThirdParagraphCheck()
        {
            ExpectHeader("What the published information will look like");
            Expect(What.Contains, "You can see examples of published statement summaries on ");
            ExpectLink("Find a modern slavery statement");

            //todo check link address
            //BelowHeader("").ExpectXpath

            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(45)]
        public async Task ExpectAndUseContinue()
        {
            Expect("Continue");
            Click("Continue");
            await AxeHelper.CheckAccessibilityAsync(this).ConfigureAwait(false);
            ExpectHeader("Transparency and modern slavery");

            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}