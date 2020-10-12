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
using System.Linq;

namespace ModernSlavery.Hosts.Web.Tests
{
    [TestFixture]
    public class Registration_Public_Start_Registration : CreateAccount
    {
        const string _firstname = Create_Account.roger_first; const string _lastname = Create_Account.roger_last; const string _title = Create_Account.roger_job_title; const string _email = Create_Account.roger_email; const string _password = Create_Account.roger_password;

        public Registration_Public_Start_Registration() : base(_firstname, _lastname, _title, _email, _password)
        {
        }

        protected Organisation org;
        [OneTimeSetUp]
        public async Task SetUp()
        {
            org = this.Find<Organisation>(org => org.SectorType.IsAny(SectorTypes.Public) && !org.UserOrganisations.Any());;


            org = this.Find<Organisation>(org => org.SectorType.IsAny(SectorTypes.Public) && org.GetLatestActiveScope().ScopeStatus.IsAny(ScopeStatuses.PresumedOutOfScope, ScopeStatuses.PresumedInScope) && org.LatestRegistrationUserId == null && !org.UserOrganisations.Any());
        }

        [Test, Order(20)]
        public async Task NavigateToOrgPage()
        {
            Goto("/manage-organisations");
            Click("Register an organisation");

            ExpectHeader("Registration Options");

            ClickLabel("Public Sector Organisation");

            Click("Continue");

            ExpectHeader("Find your organisation");

            await Task.CompletedTask;
                }


        [Test, Order(22)]
        public async Task SearchForOrg()
        { 
            SetXPath("//input[@id='SearchText']").To(org.OrganisationName);
            Click(The.Bottom, "Search");


            Expect("Organisation name and registered address");
            ExpectRow(That.Contains, org.OrganisationName);
            

            //message should not appear with single result 
            ExpectNo(What.Contains, "Showing 1-");

            await Task.CompletedTask;
        }
        [Test, Order(24)]

        public async Task SelectOrg()
        {
            ClickButton(The.Top, That.Contains, "Choose");

            ExpectHeader("Your organisation's address");
            ExpectText("Enter the correspondence address of the organisation you want to register.");

            await Task.CompletedTask;

        }
    }
}