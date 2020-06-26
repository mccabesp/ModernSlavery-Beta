using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class UserUsesEmailFunctionOfContactUs : UITest
    {
        [TestMethod]
        public override void RunTest()
        {
            //Navigation to home search page

            ExpectHeader("Search and compare Modern Slavery statements");
            Click(The.Bottom, "Contact Us");

            ExpectText("If you are an employer you can contact the Modern Slavery team about the service or registration issues you're having at gpg.reporting@cabinetoffice.gov.uk");

            ClickText("gpg.reporting@cabinetoffice.gov.uk");

            //NEED TO CHECK EMAIL SERVICE AND THAT THIS SETS UP AN EMAIL TO MSU HELP DESK Modernslaverystatements@homeoffice.gov.uk 


        }
    }
}