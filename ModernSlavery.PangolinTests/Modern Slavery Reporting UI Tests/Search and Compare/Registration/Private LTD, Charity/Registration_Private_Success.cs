using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class Registration_Private_Success : UITest
    {
        [TestMethod]
        public override void RunTest()
        {
            //Run<Create_Account_Success>();

            LoginAs<RogerReporter>();

            Click("Register an organisation");

            ExpectHeader("Registration Options");

            ClickLabel("Private Limited Company, Charity");

            Click("Continue");

            ExpectHeader("Find your organisation");

            Set("SearchText").To(Registration.OrgName_InterFloor);
            Click("Search");

            AtRow(Registration.OrgName_InterFloor).Click("Choose organisation");
           
            ExpectHeader("Confirm your organisation’s details");

            AtLabel("Organisation name").Expect(Registration.OrgName_InterFloor);
            AtLabel("Registered address").Expect(Registration.RegisteredAddress_InterFloor);
            AtLabel("Business Sectors (SIC Codes)").Expect(Registration.SicCodes_InterFloor);
            Click("Confirm");

            ExpectHeader("We`re sending a PIN by post to the following name & address:");

            //todo confirm page
            Expect(create_account.roger_first + " " + create_account.roger_last + " (" + create_account.roger_job_title + ")");
            Expect(Registration.Address1_InterFloor);
            Expect(Registration.Address2_InterFloor);
            Expect(Registration.Address3_InterFloor);
            Expect(Registration.PostCode_InterFloor);

        }
    }
}