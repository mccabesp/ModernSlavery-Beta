using Pangolin;

namespace Modern_Slavery_Reporting_UI_Tests
{
    public class RemoveUsersPageForMultipleUsers : UITest
    {
        public override void RunTest()
        {
            //Create multiple registered users for organisation
            //Login as registered user for organisation
            ExpectHeader("Select an organisation");
            AtColumn("Organisation name").Row("Virgin Media");
            Click("Virgin Media");

            ExpectHeader("Manage your organisations reporting");
            ExpectHeader("Registered Users");
            Expect("The following people are registered to report Modern Slavery statement for this organisation");
            ExpectNo("You are the only person registered to report for this organisation.");

            Click(The.Top, "Remove user from reporting");
            ExpectHeader("Confirm removal of user");

           
        }
    }
}