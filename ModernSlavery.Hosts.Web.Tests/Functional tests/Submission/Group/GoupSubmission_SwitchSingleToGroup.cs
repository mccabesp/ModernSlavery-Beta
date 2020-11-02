using Geeks.Pangolin;
using ModernSlavery.Core.Entities;
using ModernSlavery.Testing.Helpers.Extensions;
using NUnit.Framework;
using System.Threading.Tasks;

namespace ModernSlavery.Hosts.Web.Tests
{
    [TestFixture, Ignore("Waiting on 4773")]

    public class GoupSubmission_SwitchSingleToGroup : Submission_Complete_Mandatory_Sections

        //Information needing to be added: A second variant organisation to add to grouping Fly Jet Australia
    {
        //[OneTimeSetUp]
        //public async Task OTSetUp()
        //{
        //    TestData.Organisation = TestRunSetup.TestWebHost
        //        .Find<Organisation>(org => org.GetLatestActiveScope().ScopeStatus.IsAny(ScopeStatuses.PresumedOutOfScope, ScopeStatuses.PresumedInScope) && o.LatestRegistrationUserId == null));
        //    //&& !o.UserOrganisations.Any(uo => uo.PINConfirmedDate != null)
        //}

        [OneTimeSetUp]
        public async Task SetUp()
        {
            org = this.Find<Organisation>(org => org.LatestRegistrationUserId == null);

        }

        [Test, Order(50)]
        public async Task NavigateToSubmissionPage()
        {
            Click("Manage organisations");
            Click(org.OrganisationName);
            Click("Continue");
            ExpectHeader("Review before submitting");

            await Task.CompletedTask;
        }

        [Test, Order(52)]
        public async Task EnsureSectionsAreCompleted()
        {
            //mandatory sections should be completed
            RightOfText("Your modern slavery statement").ExpectText("Completed");
            RightOfText("Areas covered by your modern statement").ExpectText("Completed");
            await Task.CompletedTask;
        }

        [Test, Order(54)]
        public async Task EnsureOptionalSectionsAreIncomplete()
        {
            //all other sections incomplete 

            RightOfText("Your organisation").ExpectText("Not Completed");
            RightOfText("Policies").ExpectText("Not Completed");
            RightOfText("Supply chain risks and due diligence (part 1)").ExpectText("Not Completed");
            RightOfText("Supply chain risks and due diligence (part 2)").ExpectText("Not Completed");
            RightOfText("Training").ExpectText("Not Completed");
            RightOfText("Monitoring progress").ExpectText("Not Completed");
            await Task.CompletedTask;
        }

        [Test, Order(56)]
        public async Task NavigateToConversionPage()
        {
            Expect("Is this statement for a group of organisations?");
            ClickText("Is this statement for a group of organisations?");

            ExpectText(That.Contains, "If your statement is for a group of organisations, you need to ");
            Expect(What.Contains, "specify it’s for a group");
            ExpectText(That.Contains, "and tell us which organisations are in the group.");
            ClickText(That.Contains, "specify it’s for a group");
            ExpectHeader("Who is your statement for?");

            await Task.CompletedTask;
        }

        [Test, Order(57)]
        public async Task SelectConversionToAGroupSubmission()
        {
            //Expect("a single organisation");
            //Expect("a group of organisations");
            //Click("a group of organisations");

                    SubmissionHelper.GroupOrSingleScreenComplete(this, true, org.OrganisationName, "2020");

            Click("Continue");
            ExpectHeader("Which organisations are included in your group statement?");

            await Task.CompletedTask;
        }

        [Test, Order(58)]
        public async Task AddAndConfirmGroup()
        {
            Set("Search").To(TestData.Organisations[2].OrganisationName);
            Click("Search ");
            Expect(TestData.Organisations[2].OrganisationName);
            AtRow(TestData.Organisations[2].OrganisationName).Click("Include");
            Expect("1 orgnanisation included");
            Expect("View your group");
            Click("View your group");
            Click("Confirm and continue");

          
            await Task.CompletedTask;
        }       
    }
}