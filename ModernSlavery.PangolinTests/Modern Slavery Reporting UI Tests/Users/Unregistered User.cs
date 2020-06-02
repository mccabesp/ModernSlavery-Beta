using Pangolin;

namespace Modern_Slavery_Reporting_UI_Tests
{
    public class UnregisteredUser : UITest
    {
        public override void RunTest()
        {
            Logout();

            AssumeTime("12:00");
            AssumeDate("01/07/2020");

            Goto("/");
            ExpectHeader("Modern slavery statement reporting service");

        }
    }
}