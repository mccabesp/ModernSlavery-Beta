using Geeks.Pangolin;
using NUnit.Framework;
using System.Threading.Tasks;
using ModernSlavery.Testing;
using ModernSlavery.Core.Entities;
using ModernSlavery.Testing.Helpers.Extensions;

namespace ModernSlavery.Hosts.Web.Tests
{
    [TestFixture]


    public class Submission_Areas_Cancel_Unsaved_Data_Exit : Private_Registration_Success
    {
        [OneTimeSetUp]
        public async Task SetUp()
        {
            TestData.Organisation = this.Find<Organisation>(org => org.LatestRegistrationUserId == null);
            //&& !o.UserOrganisations.Any(uo => uo.PINConfirmedDate != null)
        }
        [Test, Order(40)]
        public async Task StartSubmission()
        {
            ExpectHeader("Select an organisation");

            Click(TestData.OrgName);


            ExpectHeader(That.Contains, "Manage your modern slavery statement submissions");

            Click("Start Draft");


            ExpectHeader("Before you start");
            Click("Start now");

            ModernSlavery.Testing.Helpers.Extensions.SubmissionHelper.GroupOrSingleScreenComplete(this, OrgName: TestData.OrgName);
            await Task.CompletedTask;
        }

        [Test, Order(42)]
        public async Task NavigateToAreasPage()
        {
            ExpectHeader("Your modern slavery statement");


            Click("Continue");

            ExpectHeader("Areas covered by your modern slavery statement");

            await Task.CompletedTask;
        }
        [Test, Order(44)]
        public async Task FillInDetailsOnAreasPage()
        {

            ClickLabel(The.Top, "No");
            Set("Please provide details").To("Here are the details");
            await Task.CompletedTask;
        }

        [Test, Order(46)]
        public async Task CancellingLeadsToUnsavedDataScreen()
        {
            //cancel in this route should return to unsaved data screen
            Click("Cancel");

            ExpectHeader("You have unsaved data, what do you want to do?");

            await Task.CompletedTask;
        }

        [Test, Order(48)]
        public async Task ExitWithoutSaving()
        {
            ExpectHeader("You have unsaved data, what do you want to do?");

            Click("Exit and lose changes");

            ExpectHeader(That.Contains, "Manage your modern slavery statement submissions");

            await Task.CompletedTask;
        }

        [Test, Order(50)]
        public async Task NavigateBackToAreasPage()
        {
            Click("Start Draft");

            ExpectHeader("Before you start");
            Click("Start now");

            ModernSlavery.Testing.Helpers.Extensions.SubmissionHelper.GroupOrSingleScreenComplete(this, OrgName: TestData.OrgName);

            ExpectHeader("Your modern slavery statement");

            Click("Continue"); 
            
            ExpectHeader(That.Contains, "Areas covered by your modern slavery statement");
            await Task.CompletedTask;
        }

        [Test, Order(52)]
        public async Task VerifyDataIsntSaved()
        {
            //data should not have been saved 

            ExpectNo("Here are the details");
            await Task.CompletedTask;

        }
    }
}