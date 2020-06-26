using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class UserChecksNavigationToNewTabGuidanceLink : UITest
    {
        [TestMethod]
        public override void RunTest()
        {

            //Navigation to home search page

            ExpectHeader("Search and compare Modern Slavery statements");
            Click(The.Bottom, "Contact Us");

            ExpectHeader("Contact Us");
            //Check navigation to Guidance link
            ClickLink("Modern Slavery reporting.");
            ExpectUrl("https://www.gov.uk/guidance/publish-an-annual-modern-slavery-statement");
            ExpectHeader("Publish an annual modern slavery statement");
            ExpectNoHeader("Contact Us");
        }
    }
}