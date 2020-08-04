using Geeks.Pangolin;
using NUnit.Framework;
using System.Threading.Tasks;

namespace ModernSlavery.Hosts.Web.Tests
{
    [TestFixture]

    public class Submission_Areas_Back_Button_Check : Private_Registration_Success
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
        public async Task YourModernSlaveryStatement()
        {

            ExpectHeader("Your modern slavery statement");


            Click("Continue");
            await Task.CompletedTask;
        }

        [Test, Order(44)]
        public async Task AreasCoveredByYourModernSlaveryStatement()
        {
            ExpectHeader("Areas covered by your modern slavery statement");

            await Task.CompletedTask;
        }


        [Test, Order(46)]
        public async Task VerifyBackButtonNavigation()
        {
            Click("Back"); 
            ExpectHeader("Your modern slavery statement");
            await Task.CompletedTask;

        }
    }
}