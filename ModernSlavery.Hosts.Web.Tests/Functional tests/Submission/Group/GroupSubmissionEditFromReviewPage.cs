using Geeks.Pangolin;
using NUnit.Framework;
using System.Threading.Tasks;

namespace ModernSlavery.Hosts.Web.Tests
{
    [TestFixture]

    public class GroupSubmissionEditFromReviewPage : GroupSubmission_SwitchSingleToGroup

    //DATA NEEDED: a third Group org to swap out for - Fly Jet Sweeden
    {
        [Test, Order(80)]
        public async Task NavigateToReviewAndEditOrganisationsPage()
        {
            Click(The.Top, "Edit and republish");
            ExpectHeader("Add your 2020 modern slavery statement to the registry");
            ClickLink("Organisations covered by the statement");
            ExpectHeader("Review the organisations in your group statement");

            await Task.CompletedTask.ConfigureAwait(false);
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

            Click("Continue");
            ExpectHeader("Review the organisations in your group statement");
            Click("Continue");

            ExpectHeader("Add your 2020 modern slavery statement to the registry");

            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}