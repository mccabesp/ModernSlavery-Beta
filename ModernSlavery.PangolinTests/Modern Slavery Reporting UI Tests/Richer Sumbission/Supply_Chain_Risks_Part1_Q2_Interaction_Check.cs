using Pangolin;
using Pangolin.Helper.UIContext;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class Supply_Chain_Risks_Part1_Q2_Interaction_Check : UITest
    {
        [TestMethod]
        public override void RunTest()
        {
            LoginAs<RogerReporter>();

            Submission_Helper.NavigateToSupplyChainRisks1(this, Submission.OrgName_Blackpool, "2019/2020");
            ExpectHeader("Supply Chain Risks and due diligence");

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