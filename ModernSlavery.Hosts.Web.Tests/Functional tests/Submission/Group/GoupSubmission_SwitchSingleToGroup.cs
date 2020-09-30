using Geeks.Pangolin;
using NUnit.Framework;
using System.Threading.Tasks;

namespace ModernSlavery.Hosts.Web.Tests
{
    [TestFixture, Ignore("Group")]

    public class GoupSubmission_SwitchSingleToGroup : Submission_Complete_Mandatory_Sections

        //Information needing to be added: A second variant organisation to add to grouping Fly Jet Australia
    {
        [Test, Order(60)]
        public async Task NavigateToSubmissionPage()
        {
            Click("Manage organisations");
            Click(TestData.OrgName);
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
            Click("Is this statement for a group of organisations?");

            ExpectText("If your statement is gor a group of organisations, you need to specify it's for a group and tell us which organisations are in the group.");
            ClickText("specify it's for a group");
            ExpectHeader("Who is your statement for?");

            await Task.CompletedTask;
        }

        [Test, Order(57)]
        public async Task SelectConversionToAGroupSubmission()
        {
            Expect("a single organisation");
            Expect("a group of organisations");
            Click("a group of organisations");

            Click("Continue");
            ExpectHeader("Which organisations are included in your group statement?");

            await Task.CompletedTask;
        }

        [Test, Order(58)]
        public async Task AddAndConfirmGroup()
        {
            Set("Search").To("Fly Jet");
            Click("Search ");
            Expect("Fly Jet Australia");
            AtRow("Fly Jet Australia").Click("Include");
            Expect("1 orgnanisation included");
            Expect("View your group");
            Click("View your group");
            Click("Confirm and continue");

          
            await Task.CompletedTask;
        }       
    }
}