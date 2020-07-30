using Geeks.Pangolin;
using NUnit.Framework;
using System.Threading.Tasks;

namespace ModernSlavery.Hosts.Web.Tests
{
    [TestFixture, Ignore("Awaiting Submission merge")]

    public class Submission_Areas_Back_Button_Check : Private_Registration_Success
    {
        [Test, Order(40)]
        public async Task NavigateToOrgRep()
        {
            ExpectHeader("Select an organisation");

            Click(Submission.OrgName_Blackpool);

            ExpectHeader("Manage your organisations reporting");
            await Task.CompletedTask;
        }

        [Test, Order(42)]
        public async Task StartSubmission()
        {
            Click("Draft Report");

            ExpectHeader("Before you start");
            Click("Start now");

            ExpectHeader("Your modern slavery statement");

            Click("Save and continue");

            ExpectHeader("Areas covered by your modern slavery statement");


            await Task.CompletedTask;
        }


        [Test, Order(44)]
        public async Task VerifyBackButtonNavigation()
        {
            Click("Back"); 
            ExpectHeader("Your modern slavery statement");

        }
    }
}