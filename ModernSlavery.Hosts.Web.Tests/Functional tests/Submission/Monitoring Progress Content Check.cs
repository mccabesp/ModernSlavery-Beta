//using Pangolin;
//using Microsoft.VisualStudio.TestTools.UnitTesting;

//namespace Modern_Slavery_Reporting_UI_Tests
//{
//    [TestClass]
//    public class MonitoringProgressContentCheck : UITest
//    {
//        [TestMethod]
//        public override void RunTest()
//        {
//            Run<Fastrack_Registration_Success>();
//            //Run preconditions to fill out submission up to monitoring progress page
//            LoginAs<RogerReporter>();

//            ExpectHeader("Select an organisation");

//            Click(Submission.OrgName_Blackpool);

//            ExpectHeader("Manage your organisations reporting");
//            Click("Continue");
//            //NEED TO DOUBLE CHECK THE NAVIGATION FOR THIS

//            ExpectHeader("Review before submitting");
//            Expect("Monitoring Progress");
//            Click("Monitoring Progress");
//            ExpectHeader("Monitoring progress");

//            //Check fields are voluntary
//            Click("Continue");
//            ExpectHeader(That.Contains, "Review");
//            Click("Back");
//            ExpectHeader("Monitoring progress");

//            Expect("Does you modern slavery statement include goals relating to how you will prevent modern slavery in your operations and supply chains? ");
//            Expect("Yes");
//            Expect("No");
//            Click("Yes");

//            Expect("How is your organisation measuring progress towards these goals?");
//            Set("How is your organisation measuring progress towards these goals?").To("Here is how we are doing this");

//            Expect("What were your key achievements in relation to reducing modern slavery during the period covered by this statement?");
//            Set("What were your key achievements in relation to reducing modern slavery during the period covered by this statement?").To("Here are our key achievments");

//            Expect("How many years has your organisation been producing modern slavery statements?");
//            Expect("If your statement is for a group of organisations, please select the answer that applies to the organisation with the longest history of producing statements.");
//            Click("This is the first time");
//            ExpectCheck("This is the first time");
//            System.Threading.Thread.Sleep(1000);
//            Click("This is the first time");
//            ExpectNoCheckbox("This is the first time");
//            Click("1-5 years");
//            Click("More than 5 years");
//            ExpectNoCheckbox("1-5 years");
//            ExpectCheck("More than 5 years");
//            Click("Continue");
//            ExpectHeader(That.Contains, "Review");

//        }
//    }
//}