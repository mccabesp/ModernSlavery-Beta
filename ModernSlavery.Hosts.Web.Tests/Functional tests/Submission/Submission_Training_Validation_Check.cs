using Geeks.Pangolin;
using NUnit.Framework;
using System.Threading.Tasks;

namespace ModernSlavery.Hosts.Web.Tests
{
    [TestFixture, Ignore("Temporary Ignore")]


    public class Submission_Training_Validation_Check : Private_Registration_Success
    {

        public async Task NavigateToTrainingPage()
        {
            Submission_Helper.NavigateToTraining(this, TestData.OrgName, "2019 to 2020");

            ExpectHeader("Training");
            await Task.CompletedTask;
        }


        [Test, Order(42)]
        public async Task SelectOtherOption()
        {
            //if chosing "other" details must be provided
            ClickLabel("Other");
            ClearField("OtherTraining");
            await Task.CompletedTask;
        }

        [Test, Order(44)]
        public async Task SubmittingFormWithoutOptionsCausesValidation()
        {
            Click("Continue");

            Expect("There is a problem");
            Expect("Missing details");

            BelowField("OtherTraining").Expect("Enter details");
            await Task.CompletedTask;
        }

        [Test, Order(46)]
        public async Task FormCanBeSubmittedWithDetailsGiven()
        {
            Set("OtherTraining").To("details");
            Click("Continue");
            ExpectHeader("Monitoring progress");
            await Task.CompletedTask;

        }
    }
}