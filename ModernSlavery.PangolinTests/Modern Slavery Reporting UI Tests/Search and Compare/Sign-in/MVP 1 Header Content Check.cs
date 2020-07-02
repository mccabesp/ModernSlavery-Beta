using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class MVP1HeaderContentCheck : UITest
    {
        [TestMethod]
        public override void RunTest()
        {
            //user
            LoginAs<RogerReporter>();

            //todo check other navigationsmoder
            Expect("Sign out");
            ExpectNo("Manage organisations");
            ExpectHeader("Submit modern slavery statement");

            ExpectNoLink("Submit modern slavery statement");
        }
    }
}