using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class Submission_Training_Content_Check : UITest
    {
        [TestMethod]
        public override void RunTest()
        {

            //Run<Fastrack_Registraion_Success>();

            LoginAs<RogerReporter>();

            Submission_Helper.NavigateToTraining(this, Submission.OrgName_Blackpool, "2019/2020");

            ExpectHeader("Training");

            Expect("Have you provided training on modern slavery and trafficking during the past year, or any other activities to raise awareness? If so, who was this for?");
            Expect("Select all that apply");


        

            ExpectLabel("All");
            ExpectLabel("Procurement");
            ExpectLabel("Human Resources");
            ExpectLabel("C-Suite");
            ExpectLabel("Whole organisation");
            ExpectLabel("Suppliers");
            ExpectLabel("Other");

            ExpectField("Please specify");

            ExpectButton("Save and continue");
            ExpectButton("Cancel"); Run<Fastrack_Registraion_Success>();

            LoginAs<RogerReporter>();

            Submission_Helper.NavigateToTraining(this, Submission.OrgName_Blackpool, "2019/2020");

            ExpectHeader("Training");

            Expect("If you have delivered training on modern slavery in the past reporting period, who has it been delivered to?");
            Expect("(select all that apply)");
            ExpectLink("What is this?");

            ExpectLabel("All");
            ExpectLabel("Procurement");
            ExpectLabel("Human Resources");
            ExpectLabel("C-Suite");
            ExpectLabel("Whole organisation");
            ExpectLabel("Suppliers");
            ExpectLabel("Other");

            ExpectField("Please specify");

            ExpectButton("Save and continue");
            ExpectButton("Cancel");
        }
    }
}