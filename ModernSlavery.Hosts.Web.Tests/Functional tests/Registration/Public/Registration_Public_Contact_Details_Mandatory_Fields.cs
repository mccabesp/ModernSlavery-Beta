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
    public class Registration_Public_Contact_Details_Mandatory_Fields : CreateAccount
    {
        const string _firstname = Create_Account.roger_first; const string _lastname = Create_Account.roger_last; const string _title = Create_Account.roger_job_title; const string _email = Create_Account.roger_email; const string _password = Create_Account.roger_password;

        
        public Registration_Public_Contact_Details_Mandatory_Fields() : base(_firstname, _lastname, _title, _email, _password)
        {


        }

        [Test, Order(20), Ignore("Bug raised in 3112, test case to be fixed and broken down once resolved")]
        public async Task Mandatory_Fields()
        {
            ClickText("Register an organisation");
            await AxeHelper.CheckAccessibilityAsync(this).ConfigureAwait(false);
            ExpectHeader("Registration Options");

            ClickLabel("Public Sector Organisation");
            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST").ConfigureAwait(false);
            Click("Continue");

            ExpectHeader("Find your organisation");

            SetXPath("//input[@id='SearchText']").To(RegistrationTestData.OrgName_Blackpool);
            Click("Search");

            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST").ConfigureAwait(false);

            ExpectRow("Organisation name and registered address");
            ExpectRow(That.Contains, RegistrationTestData.OrgName_Blackpool);
            ExpectRow(That.Contains, "PO Box 4, Blackpool, Lancashire, United Kingdom, FY1 1NA");

            //message should not appear with single result 
            ExpectNo(What.Contains, "Showing 1-");


            ClickButton(That.Contains, "Choose");
            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST").ConfigureAwait(false);
            ExpectHeader("Address of the organisation you're reporting for");
            ExpectText("Enter the correspondence address for the organisation whose Modern Slavery statement you’re reporting.");

            //fields pre-populated
            AtField("Address 1").Expect(RegistrationTestData.Address1_Blackpool);
            AtField("Address 2").Expect(RegistrationTestData.Address2_Blackpool);
            AtField("Address 3").Expect(RegistrationTestData.Address3_Blackpool);
            AtField("Postcode").Expect(RegistrationTestData.PostCode_Blackpool);

            Click("Continue");
            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST").ConfigureAwait(false);
            ExpectHeader("Your contact details");
            ExpectText("Please enter your contact details. The Government Equalities Office may contact you to confirm your registration.");

            //fields pre-populated
            AtField("First name").Expect(Create_Account.roger_first);
            AtField("Last name").Expect(Create_Account.roger_last);
            AtField("Email address").Expect(Create_Account.roger_email);
            AtField("Job title").Expect(Create_Account.roger_job_title);
            //todo confirm phone number field
            //AtField("Telephone").Expect();

            //all fields mandatory
            ClearField("First name");
            ClearField("Last name");
            ClearField("Email address");
            ClearField("Job title");
            ClearField("Telephone");

            Click("Continue");
            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST").ConfigureAwait(false);
            Expect("Please enter your first name");
            Expect("Please enter your last name");
            Expect("Please enter your email address");
            Expect("Please enter your job title");
            //Expect("Please enter your telephone");
        }
    }
}