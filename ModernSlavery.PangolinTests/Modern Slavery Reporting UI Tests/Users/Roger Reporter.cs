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

            Set("EMail").To("...");
        }
    }
}