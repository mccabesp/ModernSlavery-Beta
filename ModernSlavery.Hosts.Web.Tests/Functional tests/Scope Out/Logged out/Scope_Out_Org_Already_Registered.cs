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

    public class Scope_Out_Org_Already_Registered : CreateAccount
    {
        const string _firstname = Create_Account.roger_first; const string _lastname = Create_Account.roger_last; const string _title = Create_Account.roger_job_title; const string _email = Create_Account.roger_email; const string _password = Create_Account.roger_password;

        string Pin;
        public Scope_Out_Org_Already_Registered() : base(_firstname, _lastname, _title, _email, _password)
        {


        }
        private Organisation org;

        [OneTimeSetUp]

        public void OTSetUp()
        {
            //(TestRunSetup.TestWebHost.GetDbContext() as Microsoft.EntityFrameworkCore.DbContext)

            org = this.Find<Organisation>(org => org.GetLatestActiveScope().ScopeStatus.IsAny(ScopeStatuses.PresumedOutOfScope, ScopeStatuses.PresumedInScope) && org.LatestRegistrationUserId == null && !org.UserOrganisations.Any());
            //&& !o.UserOrganisations.Any(uo => uo.PINConfirmedDate != null)

        }


        [Test, Order(20)]
        public async Task AddSecurityCode()
        {
            DeleteCookiesAndReturnToRoot(this);

            await AxeHelper.CheckAccessibilityAsync(this);
            await this.SetSecurityCode(org, new DateTime(2021, 6, 10));


            await Task.CompletedTask;

            await this.RegisterUserOrganisationAsync(org.OrganisationName, UniqueEmail);           

            await Task.CompletedTask;
        }

        [Test, Order(22)]
        public async Task EnterScopeURLLeadsToOrgIdentityPage()
        {            
            Goto(ScopeConstants.ScopeUrl);
            ExpectHeader("Are you legally required to publish a modern slavery statement on your website?");
            await Task.CompletedTask;
        }

        [Test, Order(24)]
        public async Task EnterEmployerReferenceAndSecurityCode()
        {
            Set("Organisation Reference").To(org.OrganisationReference);
            Set("Security Code").To(org.SecurityCode);
            await Task.CompletedTask;
        }

        [Test, Order(25)]
        public async Task ConfirmDetails()
        {
            Click("Continue");
            ExpectHeader("Confirm your organisation’s details");
            RightOfText("Organisation Name").Expect(org.OrganisationName);
            RightOfText("Organisation Reference").Expect(org.OrganisationReference);
            await Task.CompletedTask;
        }

        [Test, Order(26)]
        public async Task EnteringAlreadyRegisteredOrgDetailsLeadsToAlreadyRegisteredPage()
        {
            Click("Confirm and continue");
            ExpectHeader(That.Contains, "Your organisation has already been registered on our service");

            await Task.CompletedTask;
        }

        [Test, Order(28)]
        public async Task CheckContentOfAlreadyReigsteredPage()
        {
            Try(
                 () => Expect(What.Contains, "Someone has already registered this organisation on our service. You’ll need to "),
            () => { ExpectLink(That.Contains, "sign in or create an account"); },
            () => { Expect(What.Contains, "if you want to:"); },
            () => { Expect("check the organisation’s status, and see whether our records show it’s required or not required to publish a modern slavery statement"); },
            () => { Expect("update its status (or any other details)"); });

            await Task.CompletedTask;
        }

        
    }
}