using Geeks.Pangolin;
using ModernSlavery.Testing.Helpers.Classes;
using NUnit.Framework;
using System.Threading.Tasks;

namespace ModernSlavery.Hosts.Web.Tests
{
    [TestFixture, Ignore("Needs setup added")]

    public class ReviewGroupOrganisationsFor1To5Groups : BaseUITest
    {
        //DATA NEEDED: Report and 1-5 group organisations added to this group report
        // Fly Jet - (Fly Jet Australia, Fly Jet Sweeden, Fly Jet Switzerland, Fly Jet England)
        //Date of submissions 2020-2021 period, 01/09/2020

        public ReviewGroupOrganisationsFor1To5Groups() : base(TestRunSetup.TestWebHost, TestRunSetup.WebDriverService)
        {

        }

        [Test, Order(1)]
        public async Task CheckContentForLessThenSixGroupOrganisationReview()
        {
            SignOutDeleteCookiesAndReturnToRoot(this);

            ExpectHeader("Review the organisations in your group statement");
            ExpectText("You’ve told us your statement is for the following group of organisations. You can add more organisations to the group, or remove organisations, before confirming and continuing.");
            ExpectText("If your statement is for a single organisation, and not a group, you can return to the ‘Who is your statement for?’ page and specify a single organisation.");

            ExpectRow("Fly Jet");
            Below("Fly Jet").Expect("Hestercombe House, Cheddon Fitzpaine, Taunton, Somerset, TA2 8LG");

            Expect("Included in the 2020 to 2021 statement for Fly Jet (submitted to our service on 1 September 2020");
            Expect("Fly Jet Australia");
            Below("Fly Jet Australia").Above("Fly Jet England").Expect("Hestercombe House, Cheddon Fitzpaine, Taunton, Somerset, TA2 8LG");

            Expect("Included in the 2020 to 2021 statement for Fly Jet (submitted to our service on 1 September 2020");
            Expect("Fly Jet England");
            Below("Fly Jet England").Above("Fly Jet Sweeden").Expect("Hestercombe House, Cheddon Fitzpaine, Taunton, Somerset, TA2 8LG");

            Expect("Included in the 2020 to 2021 statement for Fly Jet (submitted to our service on 1 September 2020");
            Expect("Fly Jet Sweeden");
            Below("Fly Jet Sweeden").Above("Fly Jet Switzerland").Expect("Hestercombe House, Cheddon Fitzpaine, Taunton, Somerset, TA2 8LG");

            Expect("Included in the 2020 to 2021 statement for Fly Jet (submitted to our service on 1 September 2020");
            Expect("Fly Jet Switzerland");
            Below("Fly Jet Switzerland").Above("Confirm and continue").Expect("Hestercombe House, Cheddon Fitzpaine, Taunton, Somerset, TA2 8LG");

            Expect("Confirm and Continue");
            Expect("Select more organisations");

            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(2)]
        public async Task CheckSelectOrganisationsNoOrgNavigation()
        {
            Expect("Select organisations");
            Click("Select organisations");
            ExpectHeader("which organisations are included in your group statement?");
            Click("Back");
            ExpectHeader("Review the organisations included in your group statement");

            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(3)]
        public async Task CheckSelectOrganisationsConfirmAndContinueNavigation()
        {
            Expect("Confirm and continue");
            Click("Confirm and continue");
            ExpectHeader("Your modern slavery statement");

            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}