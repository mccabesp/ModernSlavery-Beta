using Geeks.Pangolin;
using NUnit.Framework;
using System.Threading.Tasks;

namespace ModernSlavery.Hosts.Web.Tests
{
    [TestFixture, Ignore("Temporary Ignore")]


    public class Submission_YourOrg_Content_Check : Private_Registration_Success
    {

        public async Task NavigateToYourOrgPage()
        {

            Submission_Helper.NavigateToYourOrganisation(this, TestData.OrgName, "2019 to 2020");
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