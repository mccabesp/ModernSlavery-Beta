using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class ManageFeedbackContentCheck : UITest
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

            Expect("What did you do on this service?");
            Expect("Submitted a statement");
            Expect("Viewed 1 or more statements");
            Expect("Submitted and viewed statement");

            ExpectHeader("How easy or difficult is it to use the service?");
            Expect("Very easy");
            Expect("Easy");
            Expect("Neither easy or difficult");
            Expect("Difficult");
            Expect("Very difficult");

            ExpectHeader("How can we improve the service?");

            ExpectHeader("Further feedback");
            Expect("If you`re happy for us to get in touch with you about your feedback, please provide your details below.");
            Expect("Your email address (optional)");
            Expect("Your phone number (optional)");
            Expect("If you are having difficulties with the Modern Slavery reporting service please email");
            Expect("so that we can get back to you quickly.");
            Expect("Submit");



        }
    }
}