using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class CheckContentOfContactUsPage : UITest
    {
        [TestMethod]
        public override void RunTest()
        {
            Run<Create_Account_Success>();

            LoginAs<RogerReporter>();
            //Navigation to contact us page is not on wireframes yet.

            ExpectHeader("Contact us");
            ExpectHeader("Email");
            Expect("If you are having difficulty with this service, or for general enquires email us at modernslaverystatements@homeoffice.gov.uk");

            ExpectHeader("Send feedback");
            Expect("If you`d like to leave general feedback on the service please complete our online feedback form");

            ExpectHeader("Post");
            Expect("you can contact us by post at:");
            Expect("Modern Slavery Unit");
            Expect("4th floor, Peel");
            Expect("2 Marsham Street");
            Expect("London");
            Expect("SW1P 3BT");
            Expect("Close window");

            Click("Close window");
            //EXPECT NAVIGATION THAT HASNT BEEN WIREFRAMED YET
        }
    }
}