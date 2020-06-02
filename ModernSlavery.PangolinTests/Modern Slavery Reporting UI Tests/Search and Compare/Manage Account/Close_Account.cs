using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class Close_Account : UITest
    {
        [TestMethod]
        public override void RunTest()
        {
            Run<Create_Account_Success>();

            LoginAs<RogerReporter>();

            Click("Manage account");
            ExpectHeader("Manage your account");
            Click("Close your account");

            ExpectHeader("Close your account");

            Expect("This will not impact any reports already published on the service. Other people registered to the organisation will still be able to submit and change reports.");

            Expect("You will not be able to report for any organisations you have registered");
            Expect("You will not receive communications from the Modern Slavery Reporting service");

            //only user registered for org
            Expect("Closing your account will leave one or more of your registered organisations with no one to submit on their behalf. It can take up to a week to register an organisation");

            Set("Password").To(create_account.roger_passowrd);

            Click("Close your account");

            Expect("Your account has been closed");
            Expect("You are now signed out of the Modern Slavery reporting service");
        }
    }
}