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

            Click(TestData.OrgName);


            ExpectHeader(That.Contains, "Manage your modern slavery statement submissions");

            Click("Start Draft");


            ExpectHeader("Before you start");
            Click("Start now");

            ExpectHeader("Your modern slavery statement");
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