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


namespace ModernSlavery.Hosts.Web.Tests
{
    [TestFixture, Ignore("Temporary igore")]
    public class Fastrack_Registration_Success : CreateAccount
    {
        const string _firstname = Create_Account.roger_first; const string _lastname = Create_Account.roger_last; const string _title = Create_Account.roger_job_title; const string _email = Create_Account.roger_email; const string _password = Create_Account.roger_password;
        string Pin;
        public Fastrack_Registration_Success() : base(_firstname, _lastname, _title, _email, _password)
        {


        }

        private Organisation Org;
        [Test, Order(20)]
        public async Task GoToRegistrationPage()
        {
             Org = Testing.Helpers.Extensions.OrganisationHelper.GetOrganisation(TestRunSetup.TestWebHost, Registration.OrgName_InterFloor);
            Testing.Helpers.Extensions.OrganisationHelper.GetSecurityCodeBusinessLogic(TestRunSetup.TestWebHost).CreateSecurityCode(Org, new DateTime(2021, 6, 10));
            Testing.Helpers.Extensions.OrganisationHelper.GetSecurityCodeBusinessLogic(TestRunSetup.TestWebHost).CreateSecurityCode(Org, new DateTime(2021, 6, 10));
            Click("Register an organisation");

            await ModernSlavery.Testing.Helpers.Extensions.OrganisationHelper.GetOrganisationBusinessLogic(TestRunSetup.TestWebHost).SetUniqueEmployerReferenceAsync(Org);

            ExpectHeader("Registration Options");

            ClickLabel("Fast Track");

            Click("Continue");

            ExpectHeader("Fast track registration");
            await Task.CompletedTask;
        }
        [Test, Order(22)]

        public async Task ContentCheck()
        {
            Set("Organisation reference").To(Org.EmployerReference);
            Set("Security code").To(Org.SecurityCode);

            Click("Continue");


            ExpectHeader("Confirm your organisation’s details");

            //expect organisation details
            AtRow("Organisation name").Expect(Org.OrganisationName);
            AtRow("Company number").Expect(Org.CompanyNumber);
            //AtRow("Registered address").Expect(Registration.RegisteredAddress_Millbrook);

            //using contains due to label including encoded spaces and not being detected properly
            //AtRow(That.Contains, "Business").Expect(Registration.SicCode_Milbrook.Item1);
            //AtRow(That.Contains, "Business").Below(Registration.SicCode_Milbrook.Item1).Expect(Registration.SicCode_Milbrook.Item2);
            //AtRow(That.Contains, "Business").RightOf(Registration.SicCode_Milbrook.Item2).Expect(Registration.SicCode_Milbrook.Item3);

            Click("Confirm");
            ExpectHeader("You can now publish a Modern Slavery statement on behalf of this organisation.");

            At("Employer name").Expect(Org.OrganisationName);

            Below("Employer name").ExpectText("You can also specify whether this employer is in scope of the reporting regulations.");

            Click("Continue");

            ExpectHeader("Select an organisation");

            ExpectRow(Org.OrganisationName);
            AtRow(Org.OrganisationName).Column("Organisation Status").Expect("Registration Complete");
        }
        }

        

    }
