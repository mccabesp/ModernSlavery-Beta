using Geeks.Pangolin;
using NUnit.Framework;
using System.Threading.Tasks;

namespace ModernSlavery.Hosts.Web.Tests
{
    [TestFixture, Ignore("Awaiting Submission merge")]

    public class Submission_Sectors_Interaction_Check : Private_Registration_Success
    {
        [Test, Order(40)]
        public async Task NavigateToSectorsPage()
        {

            Submission_Helper.NavigateToYourOrganisation(this, Submission.OrgName_Blackpool, "2019/2020");
            await Task.CompletedTask;
        }

        [Test, Order(42)]
        public async Task ContinueLeadsToAreasPage()
        {
            ExpectHeader("Your organisation");

            ExpectHeader("Which sector does your organisation operate in? (select all that apply)");

            ExpectLink("What is this?");

            //expect all sectors in order
            Submission_Helper.ExpectSectors(this, Submission.Sectors);

            //expect all financial options in order
            Submission_Helper.ExpectFinancials(this, Submission.Financials);

            Submission_Helper.SectorsInteractionCheck(this, Submission.Sectors);
            Submission_Helper.FinancialsInteractionCheck(this, Submission.Financials);

        }
    }
}