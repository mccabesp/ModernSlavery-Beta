using Geeks.Pangolin;
using ModernSlavery.Testing.Helpers.Extensions;
using NUnit.Framework;
using System.Threading.Tasks;

namespace ModernSlavery.Hosts.Web.Tests
{
    [TestFixture, Ignore("Temp ignore while setup fixed, logic fine")]


    public class Submission_Areas_Cancel_Unsaved_Data_Save : Private_Registration_Success
    {
        [Test, Order(40)]
        public async Task StartSubmission()
        {

            ExpectHeader("Select an organisation");

            Click(org.OrganisationName);

            SubmissionHelper.MoreInformationRequiredComplete(this, true, OrgName: org.OrganisationName);



            Click(The.Bottom, "Start Draft");

            ExpectHeader("Before you start");
            Click("Start Now");

                    ModernSlavery.Testing.Helpers.Extensions.SubmissionHelper.GroupOrSingleScreenComplete(this, OrgName: org.OrganisationName);

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
        public async Task SaveTheData()
        {
            ExpectHeader("You have unsaved data, what do you want to do?");


            Click("Exit and save changes");

            ExpectHeader(That.Contains, "Manage your modern slavery statement submissions");


            Expect("In progress");

            await Task.CompletedTask;
        }

        [Test, Order(50)]
        public async Task NavigateBackToAreasPage()
        {
            Click("Continue");

            ExpectHeader("Review before submitting");

            Click("Areas covered by your modern statement");

            ExpectHeader("Areas covered by your modern slavery statement");
            await Task.CompletedTask;
        }

        [Test, Order(52)]
        public async Task VerifyDataHasBeenSaved()
        {
            //data should have been saved 

            Expect("Here are the details");
            await Task.CompletedTask;

        }
    }
}