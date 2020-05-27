using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class Registration_Public_Contact_Details_Mandatory_Fields : UITest
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

            Set("Find").To(Registration.OrgName_Blackpool);
            Click("Search");


            ExpectRow("Organisation name and registered address");
            ExpectRow(That.Contains, Registration.OrgName_Blackpool);
            ExpectRow(That.Contains, Registration.RegisteredAddress_Blackpool);

            //message should not appear with single result 
            ExpectNo(What.Contains, "Showing 1-");


            AtRow(That.Contains, Registration.OrgName_Blackpool).Click("Choose Organisation");

            ExpectHeader("Address of the organisation you`re reporting for");
            ExpectText("Enter the correspondence address for the organisation whose Modern Slavery statement you’re reporting.");

            //fields pre-populated
            AtField("Address 1").Expect(Registration.Address1_Blackpool);
            AtField("Address 2").Expect(Registration.Address2_Blackpool);
            AtField("Address 3").Expect(Registration.Address3_Blackpool);
            AtField("Postcode").Expect(Registration.PostCode_Blackpool);

            Click("Continue");

            ExpectHeader("Your contact details");
            ExpectText("Please enter your contact details. The Government Equalities Office may contact you to confirm your registration.");

            //fields pre-populated
            AtField("First name").Expect(create_account.roger_first);
            AtField("Last name").Expect(create_account.roger_last);
            AtField("Email address").Expect(create_account.roger_email);
            AtField("Job title").Expect(create_account.roger_job_title);
            //todo confirm phone number field
            //AtField("Telephone").Expect();

            //all fields mandatory
            ClearField("First name");
            ClearField("Last name");
            ClearField("Email address");
            ClearField("Job title");
            ClearField("Telephone");

            Click("Continue");

            Expect("Please enter your first name");
            Expect("Please enter your last name");
            Expect("Please enter your email address");
            Expect("Please enter your job title");
            //Expect("Please enter your telephone");
        }
    }
}