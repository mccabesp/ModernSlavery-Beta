using Geeks.Pangolin;
using ModernSlavery.Testing.Helpers.Classes;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using System.Threading.Tasks;

namespace ModernSlavery.Hosts.Web.Tests
{
    [TestFixture]

    public class CheckIfOrganisationIsAlreadyCoveredByAStatement : BaseUITest
    {
        public CheckIfOrganisationIsAlreadyCoveredByAStatement() : base(TestRunSetup.TestWebHost, TestRunSetup.WebDriverService)
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





        [TearDown]
        public void TearDown()
        {

            if (TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed) TestRunFailed = true;

        }
        //DATA NEEDED: Organisation already covered by two or more group statements for submission year 2020-2021 - Fly Jet England published 01/06/2020 and Fly Jet Australia published 01/09/2020


        [Test, Order(1)]
        public async Task CheckContentForOrganisationsStatementPage()
        {
            SignOutDeleteCookiesAndReturnToRoot(this);

            ExpectHeader("Manage your modern slavery statements");
            AtRow("2020").Column("Status of statement on the registry").Expect("Already included in:");
            AtRow("2020").Column("Status of statement on the registry").Expect("Fly Jet Australia's 2020-2021 group submission, published on 1 Sepetember 2020");
            AtRow("2020").Column("Status of statement on the registry").Expect("Fly Jet England's 2020-2021 group submission, published on 1 June 2020");

            Above("Fly Jet Australia's").Expect("Fly Jet England's");
            Below("Fly Jet England's").Expect("Fly Jet Australia's");

            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(2)]
        public async Task StartDraftForOrgAlreadyCoveredByGroupSubmission()
        {
            AtRow("2020").Click("Start draft");
            ExpectHeader("You are included in another organisation’s submission for 2020 to 2021");
            Expect("You can still submit your own statement if you wish to do so by clicking on the ‘Continue’ button.");
            Click("Contine");

            Click("a single organisation");
            Click("Continue");
            //Navigate to save draft on review
            Click("Continue");
            Click("Continue");
            Click("Continue");
            Click("Continue");
            Click("Continue");
            Click("Continue");
            Click("Continue");
            Click("This is the first time");
            Click("Continue");
            Click("Save draft");

            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(3)]
        public async Task CheckStatusOfDraftAndGroup()
        {
            AtRow("2020").Column("Status of statement on the registry").Expect("In progress");
            AtRow("2020").Column("Status of statement on the registry").Expect("Also included in Fly Jet's 2020-2021 group submission, published on 01/06/2020.");

            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}