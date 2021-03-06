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
    public class Registration_Public_Find_Your_Org_Mandatory_Field : CreateAccount
    {
        const string _firstname = Create_Account.roger_first; const string _lastname = Create_Account.roger_last; const string _title = Create_Account.roger_job_title; const string _email = Create_Account.roger_email; const string _password = Create_Account.roger_password;

        public Registration_Public_Find_Your_Org_Mandatory_Field() : base(_firstname, _lastname, _title, _email, _password)
        {
        }
        [Test, Order(20)]
        public async Task NavigateToSearchPage()
        {
            Goto("/manage-organisations");

            await AxeHelper.CheckAccessibilityAsync(this).ConfigureAwait(false);

            Click("Register an organisation");
            await AxeHelper.CheckAccessibilityAsync(this).ConfigureAwait(false);

            ClickLabel("No");
            ClickLabel("Public sector organisation");
            Click("Continue");

            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST").ConfigureAwait(false);

            ExpectHeader("Find your organisation");
            await Task.CompletedTask.ConfigureAwait(false);
        }

 [Test, Order(22)]
        public async Task EmptySearchCausesValidation()
        {
            //clicking search without field filled should cause validaiton
            Click(The.Bottom, "Search");
            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST").ConfigureAwait(false);
            Expect("There is a problem");
            Expect("There's a problem with your search");
            Expect("Please enter an organisation to search for");

        }
    }
}