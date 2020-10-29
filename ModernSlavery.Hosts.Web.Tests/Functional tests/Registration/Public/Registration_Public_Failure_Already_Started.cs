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
    [TestFixture, Ignore("Need to review")]

    public class Registration_Public_Failure_Already_Started : Registration_Public_Success
    {
        const string _firstname = Create_Account.roger_first; const string _lastname = Create_Account.roger_last; const string _title = Create_Account.roger_job_title; const string _email = Create_Account.roger_email; const string _password = Create_Account.roger_password;

        [Test, Order(40)]
        public async Task RegisterDuplicateOrg()
        {
            Goto("/");
            await NavigateToOrgPage();
            await SearchForOrg();

            await Task.CompletedTask;
        }
        [Test, Order(42)]

        public async Task SelectingDuplicateOrgCausesValidation()
        {
            ClickButton(That.Contains, "Choose");
            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST");
            //org already started
            //error should appear
            Click("Continue");
            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST");
            Expect("The following errors were detected");
            Expect("You have already started registering for this organisation");
            await Task.CompletedTask;

        }
    }
}