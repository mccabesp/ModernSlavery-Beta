using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pangolin.Service.Dom;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class ModernSlaveryStatementPeriodCoveredFields : UITest
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

            //Period covered is a mandatory field
            Click("Save and continue");
            ExpectText("There is a problem Please enter a date");

            //Both From and To fields must be set
            Set("INSERT DAY XPATH HERE").To("01");
            Set("INSERT MONTH XPATH HERE").To("06");
            Set("INSERT YEAR XPATH HERE").To("2019");
            Click("Save and continue");
            ExpectText("Please enter a date");

            //Check date format is correct
            Set("INSERT 'TO' DAY XPATH HERE").To("01");
            Set("INSERT 'TO' MONTH XPATH HERE").To("14");
            Set("INSERT 'TO' YEAR XPATH HERE").To("2020");
            Click("Save and continue");
            ExpectText("Date format is incorrect");

            //Correct above date format         
            Set("INSERT 'TO' MONTH XPATH HERE").To("12");
            Click("Save and continue");
            ExpectNoText("Date format is incorrect");

            //Check date format of "What date was your statement approved by the board or equivalent management body?"
            Set("INSERT APPROVAL DAY XPATH HERE").To("01");
            Set("INSERT APPROVAL MONTH XPATH HERE").To("14");
            Set("INSERT APPROVAL YEAR XPATH HERE").To("2020");
            Click("Save and continue");
            ExpectText("Date format is incorrect");

           









        }
    }
}