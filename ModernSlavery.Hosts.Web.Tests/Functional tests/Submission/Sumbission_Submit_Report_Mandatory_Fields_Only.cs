using Geeks.Pangolin;
using ModernSlavery.Testing.Helpers.Extensions;
using NUnit.Framework;
using System.Threading.Tasks;

namespace ModernSlavery.Hosts.Web.Tests
{
    [TestFixture]
    public class Sumbission_Submit_Report_Mandatory_Fields_Only : Submission_Complete_Mandatory_Sections
    {
        [Test, Order(50)]
        public async Task NavigateToSubmissionPage()
        {
            Click("Your organisations");
            await AxeHelper.CheckAccessibilityAsync(this).ConfigureAwait(false);
            Click(org.OrganisationName);
            await AxeHelper.CheckAccessibilityAsync(this).ConfigureAwait(false);
            Click("Continue");
            await AxeHelper.CheckAccessibilityAsync(this).ConfigureAwait(false);
            ExpectHeader("Add your 2020 modern slavery statement to the registry");

            Expect("You have completed 5 of 11 sections.");

            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(52)]
        public async Task EnsureSectionsAreCompleted()
        {
            //mandatory sections should be completed
            RightOfText("Organisations covered by the statement").ExpectText("Completed");
            RightOfText("Statement URL, dates and sign-off").ExpectText("Completed");
            RightOfText("Recommended areas covered by the statement").ExpectText("Completed");
            RightOfText("Your organisation's sectors and turnover").ExpectText("Completed");
            RightOfText("How many years you've been producing statements").ExpectText("Completed");

            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(54)]
        public async Task EnsureOptionalSectionsAreIncomplete()
        {
            //all other sections incomplete 
            RightOfText("Policies").ExpectText("Not started");
            RightOfText("Training").ExpectText("Not started");
            RightOfText("Monitoring working conditions").ExpectText("Not started");
            RightOfText("Modern slavery risks").ExpectText("Not started");
            RightOfText("Finding indicators of modern slavery").ExpectText("Not started");
            RightOfText("Demonstrating progress").ExpectText("Not started");

            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(56)]
        public async Task SubmitFormWithOptionalSections()
        {
            ExpectHeader("Submit your answers");

            Click("I understand and agree with the above declaration");

            Click("Submit for publication");

            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST").ConfigureAwait(false);

            Expect("Submission complete");
            ExpectLink("View your published statement summary on the registry");

            Click("Finish and Sign out");

            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}