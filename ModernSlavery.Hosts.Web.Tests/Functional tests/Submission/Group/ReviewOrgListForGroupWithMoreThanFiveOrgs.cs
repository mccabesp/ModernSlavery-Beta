using Geeks.Pangolin;
using ModernSlavery.Testing.Helpers.Classes;
using NUnit.Framework;
using System.Threading.Tasks;

namespace ModernSlavery.Hosts.Web.Tests
{
    [TestFixture, Ignore("Needs setup added")]


    public class ReviewOrgListForGroupWithMoreThanFiveOrgs : BaseUITest
    {
        public ReviewOrgListForGroupWithMoreThanFiveOrgs() : base(TestRunSetup.TestWebHost, TestRunSetup.WebDriverService)
        {

        }
        //DATA NEEDED: Report and 5+ Group organisations
        //DATA NEEDED: Report 5+ group organisations added to this group report
        // Fly Jet - (Fly Jet Australia, Fly Jet Sweeden, Fly Jet Switzerland, Fly Jet England, Fly Jet France, Fly Jet Germany
        //Date of submissions 2020-2021 period, 01/09/2020
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
        public async Task CheckContentFor6PlusOrganisations()
        {
            SignOutDeleteCookiesAndReturnToRoot(this);

            ExpectHeader("Review the organisations in your group statement");
            ExpectText("You’ve told us your statement is for the following group of organisations. You can add more organisations to the group, or remove organisations, before confirming and continuing.");
            ExpectText("If your statement is for a single organisation, and not a group, you can return to the ‘Who is your statement for?’ page and specify a single organisation.");
            Expect("Included in statement (6 organisations)");
            Click("Included in statement (6 organisations)");

            await Task.CompletedTask;
        }

        [Test, Order(2)]
        public async Task ExpandOrganisationList()
        {
            Expect("Fly Jet Australia");
            Expect("Fly Jet Sweeden");
            Expect("Fly Jet Switzerland");
            Expect("Fly Jet England");
            Expect("Fly Jet France");
            Expect("Fly Jet Germany");
            Expect("Confirm and continue");
            Expect("Select more organisations");

            await Task.CompletedTask;
        }
    }
}