using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class RegistrationOptionsContentCheck : UITest
    {
        [TestMethod]
        public override void RunTest()
        {
            Run<Fastrack_Registraion_Success>();

            LoginAs<RogerReporter>();

            Click("Register an organisation", Casing.Exact);

            ExpectHeader("Registration Options", Casing.Exact);

            BelowHeader("Registration Options").ExpectText("If you have received a letter please select Fast track option", Casing.Exact);

            ExpectHeader("Select which type of organisation you would like to register", Casing.Exact);

            ExpectLabel("Fast Track", Casing.Exact);
            ExpectLabel("Private Limited Company, Charity", Casing.Exact);
            ExpectLabel("Public Sector Organisation", Casing.Exact);

            ExpectButton("Continue");
        }
    }
}