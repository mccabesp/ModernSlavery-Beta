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
    [TestFixture]
    public class Private_Registration_Success : CreateAccount
    {
        const string _firstname = Create_Account.roger_first; const string _lastname = Create_Account.roger_last; const string _title = Create_Account.roger_job_title; const string _email = Create_Account.roger_email; const string _password = Create_Account.roger_password;

        string Pin;
        public Private_Registration_Success() : base(_firstname, _lastname, _title, _email, _password)
        {


        }

        protected Organisation org;
        [OneTimeSetUp]
        public async Task OTSetUp()
        {
            //HostHelper.ResetDbScope();
            org = this.Find<Organisation>(org => org.LatestRegistrationUserId == null);
            await Task.CompletedTask;
        }
        [Test, Order(20)]
        public async Task GoToPrivateRegistrationPage()
        {
            Goto("/manage-organisations");
            Click("Register an organisation");


            ExpectHeader("Registration Options");

            ClickLabel(That.Contains, "Private or voluntary sector organisation");
            Click("Continue");

            ExpectHeader("Find your organisation");
            await Task.CompletedTask;

        }

        [Test, Order(21)]

        public async Task SearchForOrganisation()
        { 
            SetXPath("//*[@id='SearchText']").To(org.OrganisationName);
            Click("Search");

            ExpectRow(That.Contains, org.OrganisationName);

            await Task.CompletedTask;

        }
        [Test, Order(22)]

        public async Task ChooseOrganisation()
        { 
        AtRow(That.Contains, org.OrganisationName).Click(What.Contains, "Choose");

            ExpectHeader("Confirm your organisation’s details");

            AtRow("Organisation name").Expect(org.OrganisationName);
            
            //todo investigate address issue
            AtRow("Registered address").Expect(org.GetAddressString(DateTime.Now));
            Click("Confirm");

            ExpectHeader("We're sending a PIN by post to the following name and address:");
            await Task.CompletedTask;

        }
        [Test, Order(23)]

        public async Task ExtractPin() {
            Pin = WebDriver.FindElement(By.XPath("//b")).Text;
            await Task.CompletedTask;
        }

        [Test, Order(24)]

        public async Task ExpectOrgDetails()
        {

            Expect(What.Contains, Create_Account.roger_first + " " + Create_Account.roger_last + " (" + Create_Account.roger_job_title + ")");
            //Expect(What.Contains, TestData.RegisteredAddress;

            await Task.CompletedTask;

        }
        [Test, Order(25)]

        public async Task VerifyPin()
        {

            Goto("/manage-organisations");
            //Click("Manage Organisations");
            ExpectHeader("Select an organisation");


            ClickText(org.OrganisationName);


            ExpectHeader("Enter your registration PIN");

            Expect("Enter the PIN we sent you by post to finish registering this organisation.");
            Expect("You will then be able to submit information about this organisation`s modern slavery statement.");

            Set("Enter PIN").To(Pin);

            ClickText("Activate and continue");

            AtRow(org.OrganisationName).Expect("Registration complete");

            await Task.CompletedTask;

        }


    }
}