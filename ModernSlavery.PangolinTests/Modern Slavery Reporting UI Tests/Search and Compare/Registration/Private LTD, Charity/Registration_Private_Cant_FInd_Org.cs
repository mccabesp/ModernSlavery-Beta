using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class Registration_Private_Cant_FInd_Org : UITest
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

            Click("Continue");

            ExpectHeader("Confirm your organisation's details");

            AtLabel("Organisation name").Expect(Registration.OrgName_CantFind);
            AtLabel("Registered address").Expect(Registration.RegisteredAddress_Blackpool);
            AtLabel("Business Sectors (SIC Codes)").Expect("");
            AtLabel("Your name").Expect(create_account.roger_first + " " + create_account.roger_last);
            AtLabel("Email").Expect(create_account.roger_email);
            //AtLabel("Telephone").Expect(Registration.);

            Click("Continue");

            Expect("We’ve got your details.");
            Expect("We will review them and get in touch to confirm.");

            ExpectHeader("What happens next");

            Expect("The Government Equalities Office will review your details and get in touch within 5 working days to confirm your registration.");

            Expect("If we need more information to complete your registration, we will call or email you.");

            Expect("If you have not heard from us after 5 working days, please check your junk email folder.");

            Expect("For help with your registration, please contact modernslaveryteam@gov.co.uk");

            ExpectLink("Add another organisation");
            ExpectLink("Manage organisations");


        }
    }
}