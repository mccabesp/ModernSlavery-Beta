using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class UserSubmitsDraftReportAsFinalReport : UITest
    {
        [TestMethod]
        public override void RunTest()
        {
            //RUN TEST CASE THAT CREATES A DRAFT REPORT FOR ORGANISATION    
            //LOG IN AS ORGNISATION MANAGER

            //Check landing page and navigation to target area
            ExpectHeader("Select an organisation");
            AtColumn("Organisation name").Click("Virgin Media");
            ExpectHeader("Manage your organisations reporting");

            //Continue draft report
            Expect("2020/21");
            AtRow("2020/21").Click("Continue");
            ExpectHeader("Review 2020 to 2021 group report for Virgin Media");
            ExpectNo("Save draft");
            ExpectNo("Confirm and submit");

            //PRECONDITION NEEDS TO LEAVE POLICY AND TRAINING NOT FINISHED

            //Check that data points still must be correct
            Click("Policies");
            ExpectHeader("Policies");
            Click("Other");
            Click("Continue");
            Expect("There is a problem");
            Expect("Please provide detail on 'other'");
            Set("Please provide detail").To("Other detail");
            Click("Continue");

            //WORK FLOW FROM REVIEWING SECTIONS TO BE DECIDED BUT ASSUME FOR NOW YOU GO BACK TO REVIEW PAGE
            ExpectHeader("Review 2020 to 2021 group report for Virgin Media");
            Expect("Save draft");
            ExpectNo("Confirm and submit");

            //Fill out other missing section
            Click("Training");
            Click("All");
            Click("Continue");
            //WORK FLOW FROM REVIEWING SECTIONS TO BE DECIDED BUT ASSUME FOR NOW YOU GO BACK TO REVIEW PAGE
            ExpectHeader("Review 2020 to 2021 group report for Virgin Media");
            Expect("Save draft");
            Expect("Confirm and submit");

            Click("Confirm and submit");
            Expect("You've submitted your Modern Slavery statement for 2020 to 2021");

            Click("Manage organisation");
            AtRow("Virgin Media").Column("Organisation status").Expect("Registration complete");

            Click("Virgin Media");
            ExpectHeader("Manage your organisations reporting");
            AtRow("2020/21").Column("Submissions status").Expect("Published on 01/01/2021"); //Insert date of todays test case here
            AtRow("2020/21").Expect("Edit and republish");





        }
    }
}