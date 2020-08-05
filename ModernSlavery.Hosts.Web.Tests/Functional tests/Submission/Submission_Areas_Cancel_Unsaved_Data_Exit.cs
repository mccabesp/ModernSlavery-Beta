using Geeks.Pangolin;
using NUnit.Framework;
using System.Threading.Tasks;

namespace ModernSlavery.Hosts.Web.Tests
{
    [TestFixture]

    public class Submission_Areas_Cancel_Unsaved_Data_Exit : Private_Registration_Success
    {
        [Test, Order(40)]
        public async Task StartSubmission()
        {
            ExpectHeader("Select an organisation");

            Click(Submission.OrgName_InterFloor);

            ExpectHeader(That.Contains, "Manage your organisation's reporting");


            Click("Draft report");

            ExpectHeader("Before you start");
            Click("Start Now");

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

            ExpectHeader("You have unsaved data");

            await Task.CompletedTask;
        }

        [Test, Order(48)]
        public async Task ExitWithoutSaving()
        {
            ExpectHeader("What do you want to do?");

            Click("Exit without saving");

            ExpectHeader("Select an organisation");

            await Task.CompletedTask;
        }

        [Test, Order(50)]
        public async Task NavigateBackToAreasPage()
        {
            Click(Submission.OrgName_Blackpool);

            ExpectHeader("Manage your organisations reporting");

            Click("Draft Report");


            ExpectHeader("Before you start");
            Click("Start now");

            ExpectHeader("Your modern slavery statement");

            Click("Save and continue");

            ExpectHeader("Areas covered by your modern slavery statement");
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