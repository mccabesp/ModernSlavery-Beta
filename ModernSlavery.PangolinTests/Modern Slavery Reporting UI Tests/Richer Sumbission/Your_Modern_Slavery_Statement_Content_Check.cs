using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class Your_Modern_Slavery_Statement_Content_Check : UITest
    {
        [TestMethod]
        public override void RunTest()
        {
            Run<Fastrack_Registration_Success>();

            LoginAs<RogerReporter>();

            ExpectHeader("Select an organisation");

            Click(Submission.OrgName_Blackpool);

            ExpectHeader("Manage your organisations reporting");
            Expect("Provide a link to the modern slavery statement on your organisation's website");

            ExpectLink("More information");

            ExpectField("Url");

            //todo confirm format of day month year fields
            AtLabel(The.Top, "Date").ExpectField("Day");
            AtLabel(The.Top, "Date").ExpectField("Month");
            AtLabel(The.Top, "Date").ExpectField("Year");

            AtLabel("to").ExpectField("Day");
            AtLabel("to").ExpectField("Month");
            AtLabel("to").ExpectField("Year");

            Expect("What is the name of the director (or equivalent) that signed off your statement?");

            ExpectField("First name");
            ExpectField("Last name");
            ExpectField("Job title");

            ExpectHeader("What date was your statement approved by the board or equivalent management body?");

            AtLabel(The.Bottom, "Date").ExpectField("Day");
            AtLabel(The.Bottom, "Date").ExpectField("Month");
            AtLabel(The.Bottom, "Date").ExpectField("Year");


            ExpectButton("Save and continue");

            ExpectButton("Cancel");
        }
    }
}