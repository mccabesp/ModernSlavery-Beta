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
    public class Fastrack_Registration_Invalid_Reference : CreateAccount
    {
        const string _firstname = Create_Account.roger_first; const string _lastname = Create_Account.roger_last; const string _title = Create_Account.roger_job_title; const string _email = Create_Account.roger_email; const string _password = Create_Account.roger_password;

        public Fastrack_Registration_Invalid_Reference() : base(_firstname, _lastname, _title, _email, _password)
        {


        }

        [Test, Order(20)]
        public async Task GoToRegistrationPage()
        {
            Click("Register an organisation");


            ExpectHeader("Registration Options");
            await Task.CompletedTask;

        }
        [Test, Order(21)]

        public async Task EnterInvalidDetails()
        {
            ClickLabel("Fast Track");
            Click("Continue");

            ExpectHeader("Fast track registration");

            //todo ensure valid security code added here
            Set("Organisation reference").To(RegistrationTestData.InvalidEmployerReference);
            Set("Security code").To(RegistrationTestData.ValidSecurityCode);
            await Task.CompletedTask;
        }

        [Test, Order(22)]

        public async Task SubmittingFormReturnsValidationMessage()
        {
            Click("Continue");

            ExpectHeader("There is a problem");
            Expect("There's a problem with your organisation reference or security code");
            await Task.CompletedTask;
        }

        }

        

    }
