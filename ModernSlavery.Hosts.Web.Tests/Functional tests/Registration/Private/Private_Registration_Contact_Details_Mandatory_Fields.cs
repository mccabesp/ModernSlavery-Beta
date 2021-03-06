﻿using Geeks.Pangolin;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Infrastructure.Hosts;
using ModernSlavery.Testing.Helpers;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using static ModernSlavery.Core.Extensions.Web;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using ModernSlavery.Core.Entities;
using ModernSlavery.Testing.Helpers.Extensions;

namespace ModernSlavery.Hosts.Web.Tests
{
    [TestFixture]

    public class Private_Registration_Contact_Details_Mandatory_Fields : CreateAccount
    {
        const string _firstname = Create_Account.roger_first; const string _lastname = Create_Account.roger_last; const string _title = Create_Account.roger_job_title; const string _email = Create_Account.roger_email; const string _password = Create_Account.roger_password;

        string Pin;
        public Private_Registration_Contact_Details_Mandatory_Fields() : base(_firstname, _lastname, _title, _email, _password)
        {
        }

        [Test, Order(20)]
        public async Task GoToPrivateRegistrationPage()
        {
            Goto("/manage-organisations");

            await AxeHelper.CheckAccessibilityAsync(this).ConfigureAwait(false);

            Click("Register an organisation");
            await AxeHelper.CheckAccessibilityAsync(this).ConfigureAwait(false);

            ClickLabel("No");
            ClickLabel("Private or voluntary sector organisation");
            Click("Continue");

            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST").ConfigureAwait(false);

            ExpectHeader("Find your organisation");
            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(21)]
        public async Task SearchForOrganisation()
        {
            SetXPath("//*[@id='SearchText']").To(RegistrationTestData.OrgName_CantFind);
            Click("Search");

            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(22)]
        public async Task CantFindOrgLeadsToDetailsPage()
        {
            ClickText("Can't find your organisation?");
            Below("Can't find your organisation?").Click("provide us with its details");

            ExpectHeader("Details of the organisation you want to register");

            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(24)]
        public async Task ExpectOrgNameFieldPrePopulated()
        {
            AtField("Organisation name").Expect(RegistrationTestData.OrgName_CantFind);
            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(26)]
        public async Task FillReference()
        {
            Expect("Enter one or more unique references to help identify your organisation:");
            Set("Company number").To(RegistrationTestData.CompanyNumber_CantFind);
            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(28)]
        public async Task ClickingContinueNavigatesToAddressPage()
        {
            Click("Continue");
            ExpectHeader("Your organisation’s address");

            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(30)]
        public async Task FillInAddressFields()
        {
            Expect("Enter the correspondence address of the organisation you want to register.");
            // The "line 1 of 2" is not visible but is in the label
            Set("Building and street line 1 of 2").To(RegistrationTestData.Address1_Blackpool);
            // This label is visually hidden
            Set("Building and street line 2 of 2").To(RegistrationTestData.Address2_Blackpool);
            Set("Town or city").To(RegistrationTestData.Address3_Blackpool);
            Set("Postcode or zip code").To(RegistrationTestData.PostCode_Blackpool);
            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(32)]
        public async Task ClickingcContinueNavigatesToContactDetailsPage()
        {
            Click("Continue");

            ExpectHeader("Your contact details");
            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(34)]
        public async Task ExpectPrePopulatedDetailsFields()
        {
            Expect("Enter your contact details.");

            //fields pre-populated
            AtField("First name").Expect(Create_Account.roger_first);
            AtField("Last name").Expect(Create_Account.roger_last);
            AtField("Email address").Expect(UniqueEmail);
            AtField("Job title").Expect(Create_Account.roger_job_title);
            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(36)]
        public async Task ClearAllFields()
        {
            //all fields mandatory
            ClearField("First name");
            ClearField("Last name");
            ClearField("Email address");
            ClearField("Job title");
            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(38)]
        public async Task SubmittingFormResultsInValidation()
        {
            Click("Continue");

            Expect("Please enter your first name");
            Expect("Please enter your last name");
            Expect("Please enter your email address");
            Expect("Please enter your job title");
            Expect("Please enter your telephone number");

            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}