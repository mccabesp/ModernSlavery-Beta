using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class Registration_Pulbic_Address_Mandatory_Fields : UITest
    {
        [TestMethod]
        public override void RunTest()
        {
            Run<Create_Account_Success>();

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
            ExpectRow(That.Contains, Registration.RegisteredAddress_Blackpool);

            //message should not appear with single result 
            ExpectNo(What.Contains, "Showing 1-");


            ClickButton(That.Contains, "Choose");

            ExpectHeader("Address of the organisation you`re reporting for");
            ExpectText("Enter the correspondence address for the organisation whose Modern Slavery statement you’re reporting.");

            //fields pre-populated
            AtField("Address 1").Expect(Registration.Address1_Blackpool);
            AtField("Address 2").Expect(Registration.Address2_Blackpool);
            AtField("Address 3").Expect(Registration.Address3_Blackpool);
            AtField("Postcode").Expect(Registration.PostCode_Blackpool);

            //postcode and address 1 are mandatory
            ClearField("Address 1");
            ClearField("Postcode");

            Click("Continue");

            Expect("Please enter your address");
            Expect("Please enter your postcode");

        }
    }
}