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

    [TestFixture, Ignore("Awaiting Scope Merge")]

    public class Scope_Out_Org_Already_Registered : UITest
    {
        private string EmployerReference;

        [Test, Order(20)]
        public async Task AddOrgToDb()
        {
            //EmployerReference = ModernSlavery.Testing.Helpers.Testing_Helpers.AddFastrackOrgToDB(TestData.OrgName, "ABCD1234");
            //ModernSlavery.Testing.Helpers.Testing_Helpers.RegisterOrganisation("", TestData.OrgName);
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
            Set("Employer Reference").To(EmployerReference);
            Set("Security Code").To("ABCD1234");
            await Task.CompletedTask;
        }

        [Test, Order(26)]
        public async Task EnteringAlreadyRegisteredOrgDetailsLeadsToAlreadyRegisteredPage()
        {
            Click("Continue");
            ExpectHeader("Your organisation has already been registered on our service");

            await Task.CompletedTask;
        }

        [Test, Order(28)]
        public async Task CheckContentOfAlreadyReigsteredPage()
        {
            Try(
                 () => Expect(What.Contains, "Someone has already registered this organisation on our service.You’ll need to"),
            () => { ExpectLink(That.Contains, "sign in or create an account"); },
            () => { Expect(What.Contains, "if you want to:"); },
            () => { Expect("check the organisation’s status, and see whether our records show it’s required or not required to publish a modern slavery statement"); },
            () => { Expect("update its status (or any other details)"); });

            await Task.CompletedTask;
        }

        [Test, Order(26)]
        public async Task SignInOrCreateAccoutnLinkLeadsToSignInPage()
        {
            Click("Sign in or create an account");
            ExpectHeader("Sign in or create an account");

            await Task.CompletedTask;
        }
    }
}