using Geeks.Pangolin;
using NUnit.Framework;
using System.Threading.Tasks;

namespace ModernSlavery.Hosts.Web.Tests
{
    [TestFixture, Ignore("Waiting on 4773")]

    public class GroupSubmissionEditFromReviewPage : GroupSubmission_SwitchSingleToGroup

        //DATA NEEDED: a third Group org to swap out for - Fly Jet Sweeden
    {
        [Test, Order(80)]
        public async Task NavigateToReviewAndEditOrganisationsPage()
        {
            ExpectHeader("Review before submitting");
            ExpectText("You can review and edit the organisations included in this group statement, or tell us it’s for a single organisation instead.");
            ClickText("review and edit the organisations");
            ExpectHeader("Review the organisations in your group statement");

            await Task.CompletedTask;
        }

        [Test, Order(81)]
        public async Task RemoveOrgAndAddOrgToGroupList()
        {
            AtRow("Fly Jet Australia").Click("Remove");
            Click("Select more organisations");
            ExpectHeader("which organisations are included in your group statement?");

            Set("Search").To("Fly Jet");
            Click("Search");

            Expect("Fly Jet Sweeden");
            AtRow("Fly Jet Sweeden").Click("Include");
            ExpectText("1 organisation included");

            Click("View your group");
            ExpectHeader("Review the organisations in your group statement");
            Click("Confirm and continue");

            ExpectHeader("Review before submitting");

            await Task.CompletedTask;
        }
    }
}