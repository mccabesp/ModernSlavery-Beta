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
    [TestFixture, Ignore("Bug raised in 3112, test case to be fixed once resolved")]
    public class Registration_Public_Start_Reigstration : CreateAccount
    {
        const string _firstname = Create_Account.roger_first; const string _lastname = Create_Account.roger_last; const string _title = Create_Account.roger_job_title; const string _email = Create_Account.roger_email; const string _password = Create_Account.roger_password;

        public Registration_Public_Start_Reigstration() : base(_firstname, _lastname, _title, _email, _password)
        {
        }
        [Test, Order(20), Ignore("Bug raised in 3112, test case to be fixed once resolved")]
        public async Task NavigateToOrgPage()
        {
            Click("Register an organisation");

            ExpectHeader("Registration Options");

            ClickLabel("Public Sector Organisation");

            Click("Continue");

            ExpectHeader("Find your organisation");

            await Task.CompletedTask;
                }


        [Test, Order(22), Ignore("Bug raised in 3112, test case to be fixed once resolved")]
        public async Task SearchForOrg()
        { 
            SetXPath("//input[@id='SearchText']").To(Registration.OrgName_Blackpool);
            Click(The.Bottom, "Search");


            ExpectRow("Organisation name and registered address");
            ExpectRow(That.Contains, Registration.OrgName_Blackpool);
            ExpectRow(That.Contains, Registration.RegisteredAddress_Blackpool);

            //message should not appear with single result 
            ExpectNo(What.Contains, "Showing 1-");

            await Task.CompletedTask;
        }
        [Test, Order(24), Ignore("Bug raised in 3112, test case to be fixed once resolved")]

        public async Task SelectOrg()
        {
            ClickButton(That.Contains, "Choose");

            ExpectHeader("Address of the organisation you`re reporting for");
            ExpectText("Enter the correspondence address for the organisation whose Modern Slavery statement you’re reporting.");

            await Task.CompletedTask;

        }
    }
}