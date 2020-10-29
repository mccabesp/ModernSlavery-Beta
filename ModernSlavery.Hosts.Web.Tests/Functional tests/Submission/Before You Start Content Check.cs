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
            await Task.CompletedTask;
            }

            public Before_You_Start_Content_Check() : base(_firstname, _lastname, _title, _email, _password)
            {


            }

            [Test, Order(29)]
            public async Task RegisterOrg()
            {
                await this.RegisterUserOrganisationAsync(org.OrganisationName, UniqueEmail);
            }


            [Test, Order(40)]
        public async Task NavigateToBeforeYouStart()
        {

            RefreshPage();

            ExpectHeader("Select an organisation");

            Click(org.OrganisationName);
            await AxeHelper.CheckAccessibilityAsync(this);
            SubmissionHelper.MoreInformationRequiredComplete(this, true, OrgName: org.OrganisationName);

            ExpectHeader(That.Contains, "Manage your modern slavery statement submissions");

            Click(The.Bottom, "Start Draft");


            await Task.CompletedTask;
        }

        [Test, Order(41)]
        public async Task PageHeaderContent()
        {
            ExpectHeader("Before you start");
            Expect("To use this service, you will need to provide us with some basic information about your most recent modern slavery statement.");
            await Task.CompletedTask;
        }

        [Test, Order(42)]
        public async Task FirstParagraphNotExpanded()
        {
            Expect("Read more about the information you need to provide");
            ExpectNo("the name of the organisation, or group of organisations, your statement is for");
            await Task.CompletedTask;
        }

        [Test, Order(43)]
        public async Task ExpandFirstParagraphAndCheckContent()
        {
            ClickText("Read more about the information you need to provide");
            await AxeHelper.CheckAccessibilityAsync(this);
            Expect("the name of the organisation, or group of organisations, your statement is for");
            Expect("the period covered by the statement");
            Expect("who signed off the statement, and when");
            Expect("which of the recommended areas the statement covers");
            Expect("a link to the full statement on your website");

            await Task.CompletedTask;
        }

        [Test, Order(44)]
        public async Task SecondParagraphNotExpanded()
        {
            Expect("We will then ask you some additional questions about your statement. These are optional – but we strongly encourage you to complete them.");
            Expect("Read more about the additional questions");
            ExpectNo("Additional questions cover:");

            await Task.CompletedTask;
        }

        [Test, Order(45)]
        public async Task ExpandSecondParagraphForContentCheck()
        {
            ClickText("Read more about the additional questions");
            Expect("Additional questions cover:");
            Expect("the sector your organisation operates in");
            Expect("your policies and codes in relation to modern slavery");
            Expect("your supply chain risks and due diligence processes");
            Expect("staff training on modern slavery risks");
            Expect("how you use goals and key performance indicators to monitor your progress in addressing modern slavery risks");
            Expect("The information you provide us with will be published on our viewing service.");


            await Task.CompletedTask;
        }

        [Test, Order(46)]
        public async Task ExpectAndUseStartNow()
        {
            Expect("Start now");
            Click("Start now");
            await AxeHelper.CheckAccessibilityAsync(this);
            ExpectHeader("Who is your statement for?");

            await Task.CompletedTask;
        }
    }    
}