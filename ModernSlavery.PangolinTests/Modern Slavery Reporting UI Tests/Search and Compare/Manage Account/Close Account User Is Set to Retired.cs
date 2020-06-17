using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class CloseAccountUserIsSetToRetired : UITest
    {
        [TestMethod]
        public override void RunTest()
        {
            Run<Close_Account>();
            //Run precondition that sets a pending orgnisation registration for roger
            //LoginAs<adminuser>

            ExpectHeader("Administrator");
            Set("Search").To("Roger");
            Click("Search");
            ExpectHeader("Search");

            Expect("Roger Reporter");
            Below(What.Contains, "users containing Roger");
            Click("Roger Reporter");
            AtRow("Roger Reporter").Column("Status").Expect("Retired");
        }
    }
}