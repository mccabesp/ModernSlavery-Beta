using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class Submission_Sectors_Content_Check : UITest
    {
        [TestMethod]
        public override void RunTest()
        {
            //Run<Fastrack_Registraion_Success>();

            LoginAs<RogerReporter>();

            Submission_Helper.NavigateToYourOrganisation(this, Submission.OrgName_Blackpool, "2019/2020");

            ExpectHeader("Your organisation");

            ExpectHeader("Which sector does your organisation operate in? (select all that apply)");

            ExpectLink("What is this?");            
           
            //expect all sectors in order
            Submission_Helper.ExpectSectors(this, Submission.Sectors);
            
            //expect all financial options in order
            Submission_Helper.ExpectFinancials(this, Submission.Financials);

        }
    }

}