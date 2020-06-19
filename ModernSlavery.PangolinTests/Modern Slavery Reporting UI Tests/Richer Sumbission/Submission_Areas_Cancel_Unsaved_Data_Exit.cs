using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class Submission_Areas_Cancel_Unsaved_Data_Exit : UITest
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

            ClickLabel(The.Top, "Yes");

            
            //cancel in this route should return to unsaved data screen
            Click("Cancel");

            ExpectHeader("You have unsaved data");
            ExpectHeader("What do you want to do?");

            Click("Exit without saving");

            ExpectHeader("Select an organisation");


            //data should not have been saved 

            Click(Submission.OrgName_Blackpool);


            ExpectHeader("Manage your organisations reporting");

            Click("Draft Report");


            ExpectHeader("Before you start");
            Click("Start now");

            ExpectHeader("Your modern slavery statement");

            Click("Save and continue");

            ExpectHeader("Areas covered by your modern slavery statement");
        }
    }
}