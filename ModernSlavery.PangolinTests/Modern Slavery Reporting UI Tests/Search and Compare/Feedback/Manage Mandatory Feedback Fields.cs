using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class ManageMandatoryFeedbackFields : UITest
    {
        [TestMethod]
        public override void RunTest()
        {
            Run<Create_Account_Success>();

            LoginAs<RogerReporter>();

            ExpectHeader("Select an organisation");

            //Expect feedback to be on page
            ExpectHeader("Help make the service better");
            Expect("We want to understand what our users want so that we can create a better service. Complete our service and make your voice heard.");
            Expect("Complete our survey");
            Click("Complete our survery");

            //Expect correct content for the page
            ExpectHeader("Send us feedback");

            Click("Submit");
            Expect("Answers are missing");
            ExpectHeader("Send us feedback");
            ExpectNoHeader("Thank you");

            //Expect all of Q 1 - 3 are mandatory
            Click("Submitted a statement");
            Click("Submit");
            Expect("Answers are missing");
            ExpectHeader("Send us feedback");
            ExpectNoHeader("Thank you");

            Click("Very easy");
            Click("Submit");
            Expect("Answers are missing");
            ExpectHeader("Send us feedback");
            ExpectNoHeader("Thank you");

            Set("How can we improve the service?").To("Here is some advice on what would make this better");
            Click("Submit");
            ExpectNo("Answers are missing");
            ExpectNoHeader("Send us feedback");
            ExpectHeader("Thank you");

        }
    }
}