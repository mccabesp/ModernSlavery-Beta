using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class DirectorInformationMandatory : UITest
    {
        [TestMethod]
        public override void RunTest()
        {
            //Preconditions to be added later

            //*** NEED TO CHECK WHETHER VALIDATIONS FOR THIS TEST CASE ARE PASSED ON SAVE AND CONTINUE OR ON WHOLE FORM SUBMISSION ***

            //Navigation to statement page
            AtRow("Vigin Media").Column("Organisation name").Click("Virgin Media");
            ExpectHeader("Manage your organisations reporting");
            AtRow("2020/21").Column("Report Status").Click("Draft report");
            ExpectHeader("Your modern slavery statement");

            //Director sign off fields are mandatory
            Click("Save and continue");
            ExpectText("Please enter a first name");
            ExpectText("Please enter a last name");
            ExpectText("Please enter a job title");

            //Check that partial completion still throws validation
            Set("First name").To("John");
            Set("Job title").To("Director");
            Click("Save and continue");
            ExpectNoText("Please enter a first name");
            ExpectText("Please enter a last name");
            ExpectNoText("Please enter a job title");

            //Check completed fields pass successfully
            Set("Last name").To("Smith");
            Click("Save and continue");
            ExpectNoText("Please enter a first name");
            ExpectNoText("Please enter a last name");
            ExpectNoText("Please enter a job title");
        }
    }
}