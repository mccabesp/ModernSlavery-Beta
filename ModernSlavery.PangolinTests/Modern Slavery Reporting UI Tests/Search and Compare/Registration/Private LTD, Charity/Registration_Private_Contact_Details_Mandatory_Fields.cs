using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class Registration_Private_Contact_Details_Mandatory_Fields : UITest
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

            Set("SearchText").To(Registration.OrgName_CantFind);
            Click("Search");
            Click("Tell us about your organisation");

            ExpectHeader("Details of the organisation you`re reporting for");
            Set("Organisation name").To(Registration.OrgName_CantFind);
            Expect("Enter one or more unique references to help identify your organisation");
            Set("Company number").To(Registration.CompanyNumber_CantFind);
            Set("Charity number").To(Registration.CharityNumber_CantFind);
            Set("Mutual Partnership number").To(Registration.MutualPartnerShipNumber_CantFind);
            Click("Other reference");

            Set("Name or type (e.g., `DUNS `)").To(Registration.DUNS_CantFind);

            Click("Continue");
            ExpectHeader("Address of the organisation you`re reporting for");
            Expect("Enter the correspondence address for the organisation whose Modern Slavery statement you’re reporting.");
            Set("Address 1").To(Registration.Address1_Blackpool);
            Set("Address 2").To(Registration.Address2_Blackpool);
            Set("Address 3").To(Registration.Address3_Blackpool);
            Set("Postcode").To(Registration.PostCode_Blackpool);

            Click("Continue");

            Click("Continue");

            ExpectHeader("Your contact details");
            Expect("Please enter your contact details. The Government Equalities Office may contact you to confirm your registration.");

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