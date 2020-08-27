using Geeks.Pangolin;
using NUnit.Framework;
using System.Threading.Tasks;

namespace ModernSlavery.Hosts.Web.Tests
{
    [TestFixture, Ignore("Awaiting Submission merge")]

    public class Before_You_Start_Content_Check : Private_Registration_Success
    {
        [Test, Order(40)]
        public async Task NavigateToBeforeYouStart()
        {

            ExpectHeader("Select an organisation");

            Click(Submission.OrgName_Blackpool);

            ExpectHeader("Manage your organisations reporting");

            AtRow("2019/20").Click("Draft report");

            ExpectHeader("Before you start");


            await Task.CompletedTask;
        }

        [Test, Order(41)]
        public async Task PageHeaderContent()
        {
            ExpectHeader("Before you start");
            Expect("To use this service, you will need to provide us with some basic information about your most recent modern slavery statement.");
            await Task.CompletedTask;
        }

        [Test, Order(42)]
        public async Task FirstParagraphNotExpanded()
        {
            Expect("Read more about the information you need to provide");
            ExpectNo("the name of the organisation, or group of organisations, your statement is for");
            await Task.CompletedTask;
        }

        [Test, Order(43)]
        public async Task ExpandFirstParagraphAndCheckContent()
        {
            Click("Read more about the information you need to provide");

            Expect("the name of the organisation, or group of organisations, your statement is for");
            Expect("the period covered by the statement");
            Expect("who signed off the statement, and when");
            Expect("which of the recommended areas the statement covers");
            Expect("a link to the full statement on your website");

            await Task.CompletedTask;
        }

        [Test, Order(44)]
        public async Task SecondParagraphNotExpanded()
        {
            Expect("We will then ask you some additional questions about your statement.These are optional – but we strongly encourage you to complete them.");
            Expect("Read more about the additional questions");
            ExpectNo("Additional questions cover:");

            await Task.CompletedTask;
        }

        [Test, Order(45)]
        public async Task ExpandSecondParagraphForContentCheck()
        {
            Click("Read more about the additional questions");
            Expect("Additional questions cover:");
            Expect("the sector your organisation operates in");
            Expect("your policies and codes in relation to modern slavery");
            Expect("your supply chain risks and due diligence processes");
            Expect("staff training on modern slavery risks");
            Expect("how you use goals and key performance indicators to monitor your progress in addressing modern slavery risks");
            Expect("The information you provide us with will be published on our viewing service.");


            await Task.CompletedTask;
        }

        [Test, Order(46)]
        public async Task ExpectAndUseStartNow()
        {
            Expect("Start now");
            Click("Start now");
            ExpectHeader("Who is this statement for?");

            await Task.CompletedTask;
        }
    }    
}