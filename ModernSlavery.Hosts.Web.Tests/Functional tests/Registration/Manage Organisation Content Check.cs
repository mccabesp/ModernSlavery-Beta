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

    public class Manage_Orgnaisation_Content_Check : CreateAccount
    {
        const string _firstname = Create_Account.roger_first; const string _lastname = Create_Account.roger_last; const string _title = Create_Account.roger_job_title; const string _email = Create_Account.roger_email; const string _password = Create_Account.roger_password;

        string Pin;
        public Manage_Orgnaisation_Content_Check() : base(_firstname, _lastname, _title, _email, _password)
        {


        }

        [Test, Order(20)]
        public async Task CheckPageContentsManageOrganisations()
        {

            ExpectHeader("Select an organisation");

            Expect("Use this page to access your registered organisations or to register a new organisation.");

            Expect("Once you have selected an organisation you can:");
            Expect("- state whether this organisation is required to publish an annual modern slavery statement");
            Expect("-provide a link to the full modern slavery statement published on the organisations website");
            Expect("-enter information about this organisations modern slavery statement");
            Expect("-save this information as a draft, and complete it at a later date before submitting it.");

            Expect("The information that you submit will be published as a Modern slavery statement report in the `Find and view a modern slavery statement service.`");

            Expect("Registar an organisation");

            ExpectHeader("Help make the service better");
            Expect("We want to understand what our users want so that we can create a better service. Complete our service and make your voice heard.");
            Expect("Complete our survey");
            await Task.CompletedTask;
        }
    }
}