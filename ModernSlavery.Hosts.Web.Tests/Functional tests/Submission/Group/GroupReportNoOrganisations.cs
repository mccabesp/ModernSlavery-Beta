using Geeks.Pangolin;
using ModernSlavery.Testing.Helpers.Classes;
using NUnit.Framework;
using System.Threading.Tasks;

namespace ModernSlavery.Hosts.Web.Tests
{
    [TestFixture, Ignore("Group")]

    public class GroupReportNoOrganisations : BaseUITest
    {
        //DATA NEEDED: Group report submission with no current added organisations
        public GroupReportNoOrganisations() : base(TestRunSetup.TestWebHost, TestRunSetup.WebDriverService)
        {

        }

        private bool TestRunFailed = false;

        [SetUp]
        public void SetUp()
        {
            if (TestRunFailed)
                Assert.Inconclusive("Previous test failed");
            else
                SetupTest(TestContext.CurrentContext.Test.Name);
        }

        [Test, Order(1)]
        public async Task CheckContentForNoOrgReviewPage()
        {
            DeleteCookiesAndReturnToRoot(this);

            ExpectHeader("Review the organisations included in your group statement");
            ExpectText("You’ve told us your statement is for the following group of organisations. You can add more organisations to the group, or remove organisations, before confirming and continuing.");
            ExpectText("If your statement is for a single organisation, and not a group, you can return to the ‘Who is your statement for?’ page and specify a single organisation.");
            await Task.CompletedTask;
        }

        [Test, Order(2)]
        public async Task CheckSelectOrganisationsNoOrgNavigation()
        {
            Expect("Select organisations");
            Click("Select organisations");
            ExpectHeader("which organisations are included in your group statement?");
            Click("Back");
            ExpectHeader("Review the organisations included in your group statement");

            await Task.CompletedTask;
        }

        [Test, Order(3)]
        public async Task CheckSelectOrganisationsConfirmAndContinueNavigation()
        {
            Expect("Confirm and continue");
            Click("Confirm and continue");
            ExpectHeader("Your modern slavery statement");

            await Task.CompletedTask;
        }
    }
}
