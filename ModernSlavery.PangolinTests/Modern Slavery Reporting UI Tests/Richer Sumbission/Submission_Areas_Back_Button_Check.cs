using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class Submission_Areas_Back_Button_Check : UITest
    {
        [TestMethod]
        public override void RunTest()
        {
            //Run<Fastrack_Registraion_Success>();

            LoginAs<RogerReporter>();

            ExpectHeader("Select an organisation");

            Click(Submission.OrgName_Blackpool);


            ExpectHeader("Manage your organisations reporting");

            Click("Draft Report");


            ExpectHeader("Before you start");
            Click("Start now");

            ExpectHeader("Your modern slavery statement");

            Click("Save and continue");

            ExpectHeader("Areas covered by your modern slavery statement");

            Click("Back"); 
            ExpectHeader("Your modern slavery statement");

        }
    }
}