﻿using Geeks.Pangolin;
//using ModernSlavery.WebUI.Viewing.Views.ActionHub.Old.EffectiveParts;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace ModernSlavery.Hosts.Web.Tests
{
    [TestFixture]


    public class Registration_Public_Success : Registration_Public_Start_Reigstration
    {
        const string _firstname = Create_Account.roger_first; const string _lastname = Create_Account.roger_last; const string _title = Create_Account.roger_job_title; const string _email = Create_Account.roger_email; const string _password = Create_Account.roger_password;

        [Test, Order(30)]
        public async Task ExpectAddressFields()
        {


            ExpectHeader("Your organisation's address");

            var OrgAdress = TestData.Organisation.GetAddress(DateTime.Now);
            //fields pre-populated
            //todo discuss address issue - RegistrationController.Manual
            //AtField("Address 1").Expect(OrgAdress.Address1);
            //AtField("Address 2").Expect(OrgAdress.TownCity);
            //AtField("Address 3").Expect(OrgAdress.County);
            //AtField("Postcode").Expect(OrgAdress.PostCode);
            await Task.CompletedTask;
        }
        [Test, Order(31)]
        public async Task ClickingContinueNavigatesToContactDetailsPage()
        {
            Click("Continue");

            ExpectHeader("Your contact details");
            ExpectText("Enter your contact details.");
            await Task.CompletedTask;
        }

        [Test, Order(32)]
        public async Task ExpectContactDetailsFields()
        {
            //fields pre-populated
            AtField("First name").Expect(Create_Account.roger_first);
            AtField("Last name").Expect(Create_Account.roger_last);
            AtField("Email address").Expect(UniqueEmail);
            AtField("Job title").Expect(Create_Account.roger_job_title);
            Set("Telephone number").To("01414453344");
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

            RightOfText("Organisation name").Expect(TestData.Organisation.OrganisationName);
            //RightOfText("Registered address").Expect(TestData.Organisation.GetAddressString(DateTime.Now));

            ExpectRow("Your contact details");
            RightOfText("Your name").Expect(Create_Account.roger_first + " " + Create_Account.roger_last + " (" + Create_Account.roger_job_title+ ")");
            RightOfText("Email").Expect(UniqueEmail);
            RightOfText("Telephone").Expect("01414453344");
            await Task.CompletedTask;

        }
        [Test, Order(36)]
        public async Task ConfirmUkAddress()
        {
            ExpectHeader("Is this a UK address?");
            ClickLabel("Yes");
            await Task.CompletedTask;

        }

        [Test, Order(37)]
        public async Task ClickingContinueNavigatesToConfirmPage()
        {
            Click("Confirm");

            //Expect("Reporting as " + Registration.OrgName_Blackpool + " " + Registration.Address3_Blackpool);
            await Task.CompletedTask;

        }


        [Test, Order(38)]
        public async Task ExpectConfirmationDetails()
        {            
            Expect(What.Contains, "We’ve got your details.");
            Expect(What.Contains, "We will review them and get in touch to let you know if your registration was successful.");

            ExpectHeader("What happens next");
            Expect("If we need more information to complete your registration, we will call or email you.");
            Expect(What.Contains, "If there is still no email from us, or for any other help with your registration, contact");
            Expect("For help with your registration, please contact modernslaveryteam@gov.co.uk");
            ExpectXPath("//a[@href='mailto:modernslaverystatements@homeoffice.gov.uk']");
            Click(The.Bottom, "Manage Organisations");
            Expect(TestData.Organisation.OrganisationName);
            await Task.CompletedTask;

        }
    }
}