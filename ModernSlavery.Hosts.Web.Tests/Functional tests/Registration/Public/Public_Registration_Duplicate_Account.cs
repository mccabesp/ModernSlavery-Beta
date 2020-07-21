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
    [TestFixture]
    public class Public_Registration_Duplicate_Org : Public_Registration_Success
    {
        const string _firstname = Create_Account.roger_first; const string _lastname = Create_Account.roger_last; const string _title = Create_Account.roger_job_title; const string _email = Create_Account.roger_email; const string _password = Create_Account.roger_password;
               
        [Test, Order(30)]
        public async Task SearchForOrg()
        {
            Goto("/");
            await GoToPublicRegistrationPage();
            await SearchForOrganisation();

            await Task.CompletedTask;
        }
        [Test, Order(31)]

        public async Task SelectDuplicateOrg() { 
            AtRow(That.Contains, Registration.OrgName_InterFloor).Click(What.Contains, "Choose");
            await Task.CompletedTask;

        }
        [Test, Order(32)]

        public async Task ExpectErrorMessages() {

            Expect("The following errors were detected");
            Expect("You are already registered for this organisation");
            await Task.CompletedTask;

        }
     

    }
}