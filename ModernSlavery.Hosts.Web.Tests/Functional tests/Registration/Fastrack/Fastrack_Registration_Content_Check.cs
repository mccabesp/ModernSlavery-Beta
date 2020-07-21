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
    public class Fastrack_Registration_Content_Check : CreateAccount
    {
        const string _firstname = Create_Account.roger_first; const string _lastname = Create_Account.roger_last; const string _title = Create_Account.roger_job_title; const string _email = Create_Account.roger_email; const string _password = Create_Account.roger_password;

        string Pin;
        public Fastrack_Registration_Content_Check() : base(_firstname, _lastname, _title, _email, _password)
        {


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

        public async Task ContentCheck()
        {

            BelowHeader("Fast track registration").ExpectText("If you have received a letter you can enter your employer reference and security code to fast track your organisation`s registration");

            BelowHeader("Fast track registration").ExpectLabel("Employer reference");
            BelowHeader("Fast track registration").ExpectField("Employer reference");

            BelowHeader("Fast track registration").ExpectLabel("Security code");
            BelowHeader("Fast track registration").ExpectField("Security code");

            ExpectButton("Continue");
            await Task.CompletedTask;
        }
        }

        

    }
