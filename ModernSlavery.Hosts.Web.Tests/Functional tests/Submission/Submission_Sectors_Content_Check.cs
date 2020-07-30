using Geeks.Pangolin;
using NUnit.Framework;
using System.Threading.Tasks;

namespace ModernSlavery.Hosts.Web.Tests
{
    [TestFixture, Ignore("Awaiting Submission merge")]

    public class Submission_Sectors_Content_Check : Private_Registration_Success
    {
        [Test, Order(40)]
        public async Task NavigateToSectorsPage()
        {

            Submission_Helper.NavigateToYourOrganisation(this, Submission.OrgName_Blackpool, "2019/2020");
            await Task.CompletedTask;
        }

        [Test, Order(42)]
        public async Task CheckSectorPageText()
        {
            ExpectHeader("Your organisation");

            ExpectHeader("Which sector does your organisation operate in? (select all that apply)");

            ExpectLink("What is this?");            
           
        

            await Task.CompletedTask;
        }

        [Test, Order(44)]
        public async Task CheckSectorSectors()
        {

            //expect all sectors in order
            Submission_Helper.ExpectSectors(this, Submission.Sectors);

            await Task.CompletedTask;
        }

        [Test, Order(44)]
        public async Task CheckSectorFinancialOptions()
        {
            //expect all financial options in order
            Submission_Helper.ExpectFinancials(this, Submission.Financials);
            await Task.CompletedTask;
        }
        }

    }