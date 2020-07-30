using Geeks.Pangolin;
using NUnit.Framework;
using System.Threading.Tasks;

namespace ModernSlavery.Hosts.Web.Tests
{
    [TestFixture, Ignore("Awaiting Submission merge")]

    public class Submission_Areas_Cancel_Unsaved_Data_Save : Private_Registration_Success
    {
        [Test, Order(40)]
        public async Task StartSubmission()
        {
            ExpectHeader("Select an organisation");

            Click(Submission.OrgName_Blackpool);


            ExpectHeader("Manage your organisations reporting");

            Click("Draft Report");


            ExpectHeader("Before you start");
            Click("Start now");

            ExpectHeader("Your modern slavery statement");
            await Task.CompletedTask;
        }

        [Test, Order(42)]
        public async Task NavigateToAreasPage()
        {
            Click("Save and continue");

            ExpectHeader("Areas covered by your modern slavery statement");

            await Task.CompletedTask;
        }
        [Test, Order(44)]
        public async Task FillInDetailsOnAreasPage()
        {

            ClickLabel(The.Top, "Yes");
            Set("Please provide detail").To("Here are the details");
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
        public async Task SaveTheData()
        {
            ExpectHeader("What do you want to do?");

            Click("Save the data");

            ExpectHeader("Select an organisation");

            Expect("You`ve saved a draft of your Modern Slavery statement for 2020/21");

            await Task.CompletedTask;
        }

        [Test, Order(50)]
        public async Task NavigateBackToAreasPage()
        {
            Click("Continue");
            Click(Submission.OrgName_Blackpool);

            ExpectHeader("Manage your organisations reporting");

            Click("Continue");
            Click("Continue");


            ExpectHeader("Your modern slavery statement");

            Click("Save and continue");

            ExpectHeader("Areas covered by your modern slavery statement");
            await Task.CompletedTask;
        }

        [Test, Order(52)]
        public async Task VerifyDataHasBeenSaved()
        {
            //data should not have been saved 

            ExpectNo("Here are the details");
            await Task.CompletedTask;

        }
    }
}