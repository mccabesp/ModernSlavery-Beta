using Geeks.Pangolin;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Testing.Helpers.Extensions;
using NUnit.Framework;
using System.Threading.Tasks;

namespace ModernSlavery.Hosts.Web.Tests
{
    [TestFixture]

    public class Submission_Areas_Back_Button_Check : CreateAccount
    {
        const string _firstname = Create_Account.roger_first; const string _lastname = Create_Account.roger_last; const string _title = Create_Account.roger_job_title; const string _email = Create_Account.roger_email; const string _password = Create_Account.roger_password;



        [OneTimeSetUp]
        public async Task SetUp()
        {
            TestData.Organisation = TestRunSetup.TestWebHost
                .Find<Organisation>(org => org.LatestRegistrationUserId == null);

        }

        public Submission_Areas_Back_Button_Check() : base(_firstname, _lastname, _title, _email, _password)
        {


        }

        [Test, Order(29)]
        public async Task RegisterOrg()
        {
            await TestRunSetup.TestWebHost.RegisterUserOrganisationAsync(TestData.Organisation.OrganisationName, UniqueEmail);
        }


        [Test, Order(40)]
        public async Task StartSubmission()
        {
            RefreshPage();

            ExpectHeader("Select an organisation");

            Click(TestData.OrgName);


            ExpectHeader(That.Contains, "Manage your modern slavery statement submissions");

            Click("Start Draft");


            ExpectHeader("Before you start");
            Click("Start now");
            ModernSlavery.Testing.Helpers.Extensions.SubmissionHelper.GroupOrSingleScreenComplete(this, OrgName: TestData.OrgName);
            ExpectHeader("Your modern slavery statement");
            await Task.CompletedTask;
        }

        [Test, Order(42)]
        public async Task YourModernSlaveryStatement()
        {

            ExpectHeader("Your modern slavery statement");


            Click("Continue");
            await Task.CompletedTask;
        }

        [Test, Order(44)]
        public async Task AreasCoveredByYourModernSlaveryStatement()
        {
            ExpectHeader("Areas covered by your modern slavery statement");

            await Task.CompletedTask;
        }


        [Test, Order(46)]
        public async Task VerifyBackButtonNavigation()
        {
            Click("Back"); 
            ExpectHeader("Your modern slavery statement");
            await Task.CompletedTask;

        }
    }
}