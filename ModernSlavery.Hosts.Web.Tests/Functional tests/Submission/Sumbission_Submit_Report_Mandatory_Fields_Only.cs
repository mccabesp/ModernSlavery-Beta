using Geeks.Pangolin;
using NUnit.Framework;
using System.Threading.Tasks;

namespace ModernSlavery.Hosts.Web.Tests
{
    [TestFixture, Ignore("Awaiting Submission merge")]

    public class Sumbission_Submit_Report_Mandatory_Fields_Only : Submission_Complete_Mandatory_Sections
    {
        [Test, Order(50)]
        public async Task NavigateToSubmissionPage()
        {

            Submission_Helper.NavigateToSubmission(this, Submission.OrgName_Blackpool, "2020", "2021");

            await Task.CompletedTask;
        }

        [Test, Order(52)]
        public async Task EnsureSectionsAreCompleted()
        {
            //mandatory sections should be completed
            AtRow("Your modern Slavery statement").Expect("Completed");
            AtRow("Areas covered by your modern slavery statement").Expect("Completed");
            await Task.CompletedTask;
        }

        [Test, Order(54)]
        public async Task EnsureOptionalSectionsAreIncomplete()
        {
            //all other sections incomplete 

            AtRow("Your organisation").Expect("Not Completed");
            AtRow("Policies").Expect("Not Completed");
            AtRow("Supply chain risks and due diligence (part 1)").Expect("Not Completed");
            AtRow("Supply chain risks and due diligence (part 2)").Expect("Not Completed");
            AtRow("Training").Expect("Not Completed");
            AtRow("Monitoring progress").Expect("Not Completed");
            await Task.CompletedTask;
        }

        [Test, Order(56)]
        public async Task SubmitFormWithOptionalSections()
        {
            Click("Confirm and submit");

            Expect("You've submitted your Modern Slavery statement for 2019 to 2020");

            Click("Finish and Sign out");


            WaitForNewPage();

            await Task.CompletedTask;

            //todo validate submitted form

        }
    }
}