using Geeks.Pangolin;
using NUnit.Framework;
using System.Threading.Tasks;

namespace ModernSlavery.Hosts.Web.Tests
{
    [TestFixture, Ignore("Awaiting Submission merge")]

    public class Submission_Training_Content_Check : Private_Registration_Success
    {
        [Test, Order(40)]
        public async Task NavigateToTrainingPage()
        {
            Submission_Helper.NavigateToTraining(this, Submission.OrgName_Blackpool, "2019/2020");

            ExpectHeader("Training");
            await Task.CompletedTask;
        }



        [Test, Order(42)]
        public async Task CheckTrainingPageText()
        {
            Submission_Helper.NavigateToTraining(this, Submission.OrgName_Blackpool, "2019/2020");

            ExpectHeader("Training");

            Expect("If you have delivered training on modern slavery in the past reporting period, who has it been delivered to?");
            Expect("(select all that apply)");
            ExpectLink("What is this?");

            ExpectField("Please specify");

            ExpectButton("Save and continue");
            ExpectButton("Cancel");

            await Task.CompletedTask;
        }

        [Test, Order(44)]
        public async Task CheckTrainingOptions()
        {
            ExpectLabel("All");
            ExpectLabel("Procurement");
            ExpectLabel("Human Resources");
            ExpectLabel("C-Suite");
            ExpectLabel("Whole organisation");
            ExpectLabel("Suppliers");
            ExpectLabel("Other");

            await Task.CompletedTask;
        }

        
    }
}