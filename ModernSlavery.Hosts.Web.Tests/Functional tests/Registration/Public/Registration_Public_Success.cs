using Geeks.Pangolin;
using NUnit.Framework;
using System.Threading.Tasks;

namespace ModernSlavery.Hosts.Web.Tests
{
    [TestFixture, Ignore("Temporary igore")]


    public class Registration_Public_Success : Registration_Public_Start_Reigstration
    {
        const string _firstname = Create_Account.roger_first; const string _lastname = Create_Account.roger_last; const string _title = Create_Account.roger_job_title; const string _email = Create_Account.roger_email; const string _password = Create_Account.roger_password;

        [Test, Order(30)]
        public async Task ExpectAddressFields()
        {


            ExpectHeader("Address of the organisation you`re reporting for");


            //fields pre-populated
            AtField("Address 1").Expect(Registration.Address1_Blackpool);
            AtField("Address 2").Expect(Registration.Address2_Blackpool);
            AtField("Address 3").Expect(Registration.Address3_Blackpool);
            AtField("Postcode").Expect(Registration.PostCode_Blackpool);
            await Task.CompletedTask;
        }
        [Test, Order(31)]
        public async Task ClickingContinueNavigatesToContactDetailsPage()
        {
            Click("Continue");

            ExpectHeader("Your contact details");
            ExpectText("Please enter your contact details. The Government Equalities Office may contact you to confirm your registration.");
            await Task.CompletedTask;
        }

        [Test, Order(32)]
        public async Task ExpectContactDetailsFields()
        {
            //fields pre-populated
            AtField("First name").Expect(Create_Account.roger_first);
            AtField("Last name").Expect(Create_Account.roger_last);
            AtField("Email address").Expect(Create_Account.roger_email);
            AtField("Job title").Expect(Create_Account.roger_job_title);
            await Task.CompletedTask;

        }

        [Test, Order(33)]
        public async Task ClickingContinueNavigatesToOrgDetailsPage()
        {
            Click("Continue");

            ExpectHeader("Confirm your organisation’s details");
            await Task.CompletedTask;

        }

        [Test, Order(34)]
        public async Task ExpectOrgDetailsFields()
        {

            AtLabel("Organisation name").Expect(Registration.OrgName_Blackpool);
            AtLabel("Registered address").Expect(Registration.RegisteredAddress_Blackpool);
            AtLabel("Business Sectors (SIC Codes)").Expect(Registration.SicCodes_Blackpool);

            ExpectHeader("Your contact details");
            AtLabel("Your name").Expect(Create_Account.roger_first + " " + Create_Account.roger_last);
            AtLabel("Email").Expect(Create_Account.roger_email);
            AtLabel("").Expect(Registration.OrgName_Blackpool);
            await Task.CompletedTask;

        }
        [Test, Order(36)]
        public async Task ConfirmUkAddress()
        {
            ExpectHeader("Is this a UK address");
            ClickLabel("Yes");
            await Task.CompletedTask;

        }

        [Test, Order(37)]
        public async Task ClickingContinueNavigatesToConfirmPage()
        {
            Click("Continue");

            Expect("Reporting as " + Registration.OrgName_Blackpool + " " + Registration.Address3_Blackpool);
            await Task.CompletedTask;

        }


        [Test, Order(38)]
        public async Task ExpectConfirmationDetails()
        {            
            Expect("We’ve got your details.");
            Expect("We will review them and get in touch to confirm.");

            ExpectHeader("What happens next");
            Expect("The Government Equalities Office will review your details and get in touch within 5 working days to confirm your registration.");
            Expect("If we need more information to complete your registration, we will call or email you.");
            Expect("If you have not heard from us after 5 working days, please check your junk email folder.");
            Expect("For help with your registration, please contact modernslaveryteam@gov.co.uk");

            Click(The.Bottom, "Manage Organisations");
            ExpectRow(Registration.OrgName_Blackpool);
            await Task.CompletedTask;

        }
    }
}