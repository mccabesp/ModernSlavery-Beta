using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class Registration_Public_Cant_Find_Your_Org_Success : UITest
    {
        [TestMethod]
        public override void RunTest()
        {
            //Run<Create_Account_Success>();

            LoginAs<RogerReporter>();

            Click("Register an organisation");

            ExpectHeader("Registration Options");

            ClickLabel("Public Sector Organisation");

            Click("Continue");

            ExpectHeader("Find your organisation");

            SetXPath("//input[@id='SearchText']").To(Registration.OrgName_Blackpool);
            Click(The.Bottom, "Search");


            ExpectRow("Organisation name and registered address");
            ExpectRow(That.Contains, Registration.OrgName_Blackpool);
            ExpectRow(That.Contains, "PO Box 4, Blackpool, Lancashire, United Kingdom, FY1 1NA");

            //message should not appear with single result 
            ExpectNo(What.Contains, "Showing 1-");

            Click("Can't find your organisation?");
            Click("Tell us about your organisation");

            ExpectHeader(That.Contains, "Details of the organisation you're");

            //org name pre-filled by search
            AtField("Organisation name").Expect(Registration.OrgName_Blackpool);


            Expect("Enter one or more unique references to help identify your organisation:");

            Set("Company number").To("12345678");

            Click("Other reference?");
            Set("Name or type (e.g., 'DUNS ')").To("DUNS");
            Set("Unique number or value (e.g., '987654321')").To("01233345555");
            Click("Continue");
            ExpectHeader("Select your organisation");
        }
    }
}