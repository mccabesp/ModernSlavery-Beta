using Geeks.Pangolin;
using ModernSlavery.Testing.Helpers.Extensions;
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
            await AxeHelper.CheckAccessibilityAsync(this);

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
            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST");
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
            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST");
            ExpectHeader("Monitoring progress");
            await Task.CompletedTask;

        }
    }
}