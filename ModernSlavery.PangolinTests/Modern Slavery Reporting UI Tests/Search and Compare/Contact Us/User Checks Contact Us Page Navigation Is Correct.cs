using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class UserChecksContactUsPageNavigationIsCorrect : UITest
    {
        [TestMethod]
        public override void RunTest()
        {
            //Navigation to home search page

            ExpectHeader("Search and compare Modern Slavery statements");
            Click(The.Bottom, "Contact Us");

            ExpectHeader("Contact Us");

            //Check navigation to feedback form link
            ExpectText("This is a new service – your feedback will help us to improve it.");
            Click(The.Top, "feedback");
            ExpectHeader("Send us feedback");
            ExpectNoHeader("Contact Us");
            ClickText(The.Top, "Back");
            ExpectHeader("Contact Us");
            ExpectNoHeader("Send us feedback");

            //Check navigation when using close window function
            ExpectButton("Close Window");
            Click("Close Window");
            ExpectHeader("Search and compare Modern Slavery statements");
            ExpectNoHeader("Contact Us");

            
        }

    }
}