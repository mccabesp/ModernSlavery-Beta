using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class RogerReporter : UITest
    {
        [TestMethod]
        public override void RunTest()
        {
            Goto("/");
            Click("Sign in");
            Set("EMail").To("stephen@coolcorner.com");
            Set("Password").To("Cadence2007");

            Click(The.Bottom, "Sign in");
        }
    }
}