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
    public class Fastrack_Registration_Success : CreateAccount
    {
        const string _firstname = Create_Account.roger_first; const string _lastname = Create_Account.roger_last; const string _title = Create_Account.roger_job_title; const string _email = Create_Account.roger_email; const string _password = Create_Account.roger_password;
        string Pin;
        public Fastrack_Registration_Success() : base(_firstname, _lastname, _title, _email, _password)
        {


        }

        private Organisation Org;

        [OneTimeSetUp]
        public async Task OTSetUp()
        {
            //HostHelper.ResetDbScope();
            Org = this.Find<Organisation>(org => org.LatestRegistrationUserId == null);

            await this.SetSecurityCode(Org, new DateTime(2022, 01, 01));
            await Task.CompletedTask;
        }

        [Test, Order(20)]
        public async Task GoToRegistrationPage()
        {
           
            

            Click("Register an organisation");


            ExpectHeader("Registration Options");

            ClickLabel("Fast Track");

            Click("Continue");

            ExpectHeader("Fast track registration");
            await Task.CompletedTask;
        }
        [Test, Order(22)]

        public async Task ContentCheck()
        {
            Set("Organisation reference").To(Org.OrganisationReference);
            Set("Security code").To(Org.SecurityCode);

            Click("Continue");


            ExpectHeader("Confirm your organisation’s details");

            //expect organisation details
            AtRow("Organisation name").Expect(Org.OrganisationName);
            AtRow("Company number").Expect(Org.CompanyNumber);
            AtRow("Registered address").Expect(TestData.Organisation.GetAddressString(DateTime.Now));

            

            Click("Confirm");
            ExpectHeader(That.Contains, "You can now submit a modern slavery statement for this organisation.");

            RightOf("Organisation name").Expect(Org.OrganisationName);

            //Below("Employer name").ExpectText("You can also specify whether this employer is in scope of the reporting regulations.");

            Click("Continue");

            ExpectHeader("Select an organisation");

            ExpectRow(Org.OrganisationName);
            AtRow(Org.OrganisationName).Column("Registration status").Expect("Registration Complete");
        }
    }



}
