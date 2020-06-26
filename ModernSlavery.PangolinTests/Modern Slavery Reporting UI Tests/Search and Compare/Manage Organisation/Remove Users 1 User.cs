using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class RemoveUsers1User : UITest
    {
        [TestMethod]
        public override void RunTest()
        {
            //Login as registered user for organisation
            ExpectHeader("Select an organisation");
            AtColumn("Organisation name").Row("Virgin Media");
            Click("Virgin Media");

            ExpectHeader("Manage your organisations reporting");
            ExpectHeader("Registered Users");
            Expect("You are the only person registered to report for this organisation");
            Expect("If you remove yourself:");
            Expect("you will not be able to report this organisation");
            Expect("Someone else must register this organisation to report -t his can take up to a week");
            Expect("Your account will remain open");

            ExpectNo("The following people are registered to report Modern Slavery statement for this organisation");

            Click("Remove user from reporting");
            ExpectHeader("Confirm removal of user");
            

        }
    }
}