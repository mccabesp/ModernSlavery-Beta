using Geeks.Pangolin;
using NUnit.Framework;
using System.Threading.Tasks;

namespace ModernSlavery.Hosts.Web.Tests
{
    [TestFixture]

    public class Submission_Training_Content_Check : Private_Registration_Success
    {
        [Test, Order(40)]
        public async Task NavigateToTrainingPage()
        {
            Submission_Helper.NavigateToTraining(this, Submission.OrgName_InterFloor, "2019 to 2020");

            ExpectHeader("Training");
            await Task.CompletedTask;
        }



        [Test, Order(42)]
        public async Task CheckTrainingPageText()
        {
            Expect("Have you provided training on modern slavery and trafficking during the past year, or any other activities to raise awareness? If so, who was this for?");
            Expect("select all that apply");


            ExpectButton("Continue");
            ExpectButton("Cancel");

            await Task.CompletedTask;
        }

        [Test, Order(44)]
        public async Task CheckTrainingOptions()
        {
            ExpectLabel("Procurement");
            ExpectLabel("Human Resources");
            ExpectLabel("Executive level");
            ExpectLabel("Whole organisation");
            ExpectLabel("Suppliers");
            ExpectLabel("Other");



            await Task.CompletedTask;
        }
        [Test, Order(46)]
        public async Task ExpectOtherDetailsField()
        {
            ClickLabel("Other");
            ExpectLabel("Please specify"); 
            await Task.CompletedTask;
        }
    }
}