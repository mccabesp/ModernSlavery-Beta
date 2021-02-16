using Geeks.Pangolin;
using ModernSlavery.Testing.Helpers.Extensions;
using NUnit.Framework;
using System.Threading.Tasks;

namespace ModernSlavery.Hosts.Web.Tests
{
    [TestFixture]
    public class View_Submitted_Statement_From_Submission_Complete : Sumbission_Submit_Report_Mandatory_Fields_Only
    {

        [Test, Order(60)]
        public async Task ExpectNavigationLink()
        {
            Expect("Submission complete");


            ExpectLink("View your published statement summary on the registry");

            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(62)]
        public async Task NavigateToViewingService()
        {
            Click("View your published statement summary on the registry");

            ExpectHeader(org.OrganisationName + " modern slavery statement summary (2019 to 2020)");

            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(64)]
        public async Task VerifySubmission()
        {
            //todo verify submision

            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}