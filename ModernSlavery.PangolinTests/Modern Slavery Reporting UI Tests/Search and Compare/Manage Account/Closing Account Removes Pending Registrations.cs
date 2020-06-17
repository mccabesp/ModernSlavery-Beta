using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class ClosingAccountRemovesPendingRegistrations : UITest
    {
        [TestMethod]
        public override void RunTest()
        {
            Run<Close_Account>();
            //Run precondition that sets a pending orgnisation registration for roger
            //LoginAs<adminuser>

            ExpectHeader("Administrator");
            Click("Pending registrations");

            ExpectHeader("Pending Registrations");
            ExpectNoRow(That.Contains, "Roger Reporter");

        }
    }
}