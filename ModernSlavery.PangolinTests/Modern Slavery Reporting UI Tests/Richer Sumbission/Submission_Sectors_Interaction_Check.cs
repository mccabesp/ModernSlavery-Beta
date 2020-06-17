using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class Submission_Sectors_Interaction_Check : UITest
    {
        [TestMethod]
        public override void RunTest()
        {

            //Run<Fastrack_Registraion_Success>();

            LoginAs<RogerReporter>();

            Submission_Helper.NavigateToSectors(this, Submission.OrgName_Blackpool, "2019/2020");

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