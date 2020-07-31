using Geeks.Pangolin;
using Geeks.Pangolin.Helper.UIContext;
using NUnit.Framework;
using System.Threading.Tasks;

namespace ModernSlavery.Hosts.Web.Tests
{
    [TestFixture, Ignore("Awaiting Submission merge")]

    public class Supply_Chain_Risks_Part1_Q2_Interaction_Check : Private_Registration_Success
    {
        [Test, Order(40)]
        public async Task NavigateToSCRPart1()
        {

            Submission_Helper.NavigateToSupplyChainRisks1(this, Submission.OrgName_Blackpool, "2019/2020");
            ExpectHeader("Supply Chain Risks and due diligence");
            await Task.CompletedTask;
        }
        [Test, Order(42)]
        public async Task CheckAllCountries()
        {
            //check all countries for each country section
            Submission_Helper.CountrySelect(this, "Africa", Submission.African_Countries);
            Submission_Helper.CountrySelect(this, "Asia", Submission.Asian_Countries);
            Submission_Helper.CountrySelect(this, "Europe", Submission.European_Countries);
            Submission_Helper.CountrySelect(this, "North America", Submission.NorthAmerican_Countries);
            Submission_Helper.CountrySelect(this, "Oceania", Submission.Oceanic_Countries);
            Submission_Helper.CountrySelect(this, "South America", Submission.SouthAmerican_Countries);
            Submission_Helper.CountrySelect(this, "Antarctica", Submission.Antarctic_Countries);

        }

        
        }
    }