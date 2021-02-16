using Geeks.Pangolin;
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
    [TestFixture, Ignore("Cant find process is not for private orgs")]
    public class Registration_Public_Cant_Find_Your_Org_Success : CreateAccount
    {
        const string _firstname = Create_Account.roger_first; const string _lastname = Create_Account.roger_last; const string _title = Create_Account.roger_job_title; const string _email = Create_Account.roger_email; const string _password = Create_Account.roger_password;

        public Registration_Public_Cant_Find_Your_Org_Success() : base(_firstname, _lastname, _title, _email, _password)
        {
        }

        [Test, Order(20)]
        public async Task GoToPrivateRegistrationPage()
        {
            Goto("/manage-organisations");

            await AxeHelper.CheckAccessibilityAsync(this).ConfigureAwait(false);


            Click("Register an organisation");

            await AxeHelper.CheckAccessibilityAsync(this).ConfigureAwait(false);

            ExpectHeader("Did you receive a letter from us?");

            ClickLabel("No");

            ClickLabel(That.Contains, "Private or voluntary sector organisation");
            Click("Continue");
            await AxeHelper.CheckAccessibilityAsync(this).ConfigureAwait(false);


            ExpectHeader("Find your organisation");
            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(21)]
        public async Task SearchForOrganisation()
        {
            SetXPath("//*[@id='SearchText']").To(RegistrationTestData.OrgName_CantFind);
            Click("Search");

            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST").ConfigureAwait(false);

            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(22)]
        public async Task CantFindOrgLeadsToDetailsPage()
        {
            Click("Can't find your organisation?");
            Below("Can't find your organisation?").Click("provide us with its details");
            await AxeHelper.CheckAccessibilityAsync(this).ConfigureAwait(false);

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
        public async Task ClickingcContinueNavigatesToAddressPage()
        {
            Click("Continue");
            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST").ConfigureAwait(false);

            ExpectHeader("Your organisation's address");

            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(30)]
        public async Task FillInAddressFields()
        {
            Expect("Enter the correspondence address of the organisation you want to register.");
            Set("Address 1").To(RegistrationTestData.Address1_Blackpool);
            Set("Address 2").To(RegistrationTestData.Address2_Blackpool);
            Set("Address 3").To(RegistrationTestData.Address3_Blackpool);
            Set("Postcode").To(RegistrationTestData.PostCode_Blackpool);

            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(32)]
        public async Task ClickingcContinueNavigatesToContactDetailsPage()
        {
            Click("Continue");
            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST").ConfigureAwait(false);

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
            Set("Telephone number").To("01413334444");
            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(36)]
        public async Task SubmitOrg()
        {
            Click("Continue");
            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST").ConfigureAwait(false);

            ExpectHeader("Confirm your organisation’s details");
            RightOfText("Organisation name").Expect(RegistrationTestData.CompanyNumber_CantFind);
            RightOfText("Registered address").Expect(RegistrationTestData.Address1_Blackpool +", " + RegistrationTestData.Address2_Blackpool + ", " + RegistrationTestData.Address3_Blackpool + ", " + RegistrationTestData.PostCode_Blackpool);
            ExpectRow("Your contact details");
            RightOfText("Your name").Expect(Create_Account.roger_first + " " + Create_Account.roger_last + " (" + Create_Account.roger_job_title + ")");
            RightOfText("Email").Expect(UniqueEmail);
            RightOfText("Telephone").Expect("01413334444");
            Click("Confirm");

            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST").ConfigureAwait(false);

            ExpectHeader(That.Contains, "We’ve got your details.");
            ExpectHeader(That.Contains, "We will review them and get in touch to let you know if your registration was successful.");
            Click("Your organisations");

            await AxeHelper.CheckAccessibilityAsync(this).ConfigureAwait(false);

            Expect("Your registered organisations");
            RightOfText(RegistrationTestData.OrgName_CantFind).Expect("Awaiting registration approval");

            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}