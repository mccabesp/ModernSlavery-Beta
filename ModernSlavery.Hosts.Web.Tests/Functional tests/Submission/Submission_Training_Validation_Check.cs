using Geeks.Pangolin;
using NUnit.Framework;
using System.Threading.Tasks;

namespace ModernSlavery.Hosts.Web.Tests
{
    [TestFixture, Ignore("Awaiting Submission merge")]

    public class Submission_Training_Validation_Check : Private_Registration_Success
    {
        [Test, Order(40)]
        public async Task NavigateToTrainingPage()
        {
            Submission_Helper.NavigateToTraining(this, Submission.OrgName_Blackpool, "2019/2020");

            ExpectHeader("Training");
            await Task.CompletedTask;
        }


        [Test, Order(42)]
        public async Task SelectOtherOption()
        {
            //if chosing "other" details must be provided
            ClickLabel("Other");
            ClearField("Please specify");
            await Task.CompletedTask;
        }

        [Test, Order(44)]
        public async Task SubmittingFormWithoutOptionsCausesValidation()
        {
            Click("Save and continue");

            Expect("There is a problem");
            Expect("Please provide `other` details");
            await Task.CompletedTask;
        }

        [Test, Order(46)]
        public async Task FormCanBeSubmittedWithDetailsGiven()
        {
            Set("Please specify").To("details");
            Click("Save and continue");
            ExpectHeader("Monitoring progress");
        }
    }
}