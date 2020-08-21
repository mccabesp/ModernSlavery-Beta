using Geeks.Pangolin;
using NUnit.Framework;
using System.Threading.Tasks;

namespace ModernSlavery.Hosts.Web.Tests
{
    [TestFixture]

    public class Submission_Areas_Mandatory_Field_Check : Private_Registration_Success
    {
        [Test, Order(40)]
        public async Task StartSubmission()
        {
            ExpectHeader("Select an organisation");

            Click(Submission.OrgName_InterFloor);


            ExpectHeader(That.Contains, "Manage your modern slavery statement submissions");

            Click("Start Draft");


            ExpectHeader("Before you start");
            Click("Start now");

            ExpectHeader("Your modern slavery statement");
            await Task.CompletedTask;
        }

        [Test, Order(41)]
        public async Task NavigateToAreasPage()
        {
            ExpectHeader("Your modern slavery statement");


            Click("Continue");

            ExpectHeader("Areas covered by your modern slavery statement");

            await Task.CompletedTask;
        }
        [Test, Order(42)]

        public async Task OpenDetailsPopUp()
        {
            AtLabel("Your organisation’s structure, business and supply chains").ClickLabel("Yes");
            await Task.CompletedTask;
        }

        [Test, Order(44)]
        public async Task ClickingContinueTriggersValidation()
        {
            Click("Save and continue");

            ExpectHeader("There is a problem");
            Expect("Please provide the detail");

            //inline error tbc
            AtLabel("Please provide details").Expect("Please provide the detail");
        }
    }
}