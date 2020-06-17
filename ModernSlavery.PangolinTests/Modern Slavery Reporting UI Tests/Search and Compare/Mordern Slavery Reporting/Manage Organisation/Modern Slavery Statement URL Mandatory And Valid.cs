using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class ModernSlaveryStatementURLMandatoryAndValid : UITest
    {
        [TestMethod]
        public override void RunTest()
        {
            //Preconditions to be added later

            //Navigation to statement page
            AtRow("Vigin Media").Column("Organisation name").Click("Virgin Media");
            ExpectHeader("Manage your organisations reporting");
            AtRow("2020/21").Column("Report Status").Click("Draft report");
            ExpectHeader("Your modern slavery statement");

            //URL is a mandatory field
            Click("Save and continue");
            ExpectText("Please enter a URL");

            //Check only valid URL formats are accepted
            Set("URL").To("https://www.google");
            Click("Save and continue");
            ExpectText("URL is not valid");

            //Check there are no false flags for a correct result
            Set("URL").To("https://www.google.com");
            Click("Save and continue");
            ExpectNoText("URL is not valid");
        }
    }
}