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

    [TestFixture]

    public class Scope_Out_Confirm_Details_Back_Button_Check : UITest
    {
        protected string EmployerReference;

        [Test, Order(20)]
        public async Task SetSecurityCode()
        {
            TestData.Organisation = TestRunSetup.TestWebHost
                .Find<Organisation>(org => TestData.Organisation.GetLatestActiveScope().ScopeStatus.IsAny(ScopeStatuses.PresumedOutOfScope, ScopeStatuses.PresumedInScope));

            var result = Testing.Helpers.Extensions.OrganisationHelper.GetSecurityCodeBusinessLogic(TestRunSetup.TestWebHost).CreateSecurityCode(TestData.Organisation, new DateTime(2021, 6, 10));

            if (result.Failed)
            {
                throw new Exception("Unable to set security code");
            }

            await Testing.Helpers.Extensions.OrganisationHelper.SaveAsync(TestRunSetup.TestWebHost);

            await Task.CompletedTask;
        }

        [Test, Order(22)]
        public async Task EnterScopeURLLeadsToOrgIdentityPage()
        {
            Goto(TestData.ScopeUrl);
            ExpectHeader("Are you legally required to publish a modern slavery statement on your website?");
            await Task.CompletedTask;
        }

        [Test, Order(24)]
        public async Task EnterEmployerReferenceAndSecurityCode()
        {
            Set("Organisation Reference").To(TestData.Organisation.OrganisationReference);
            Set("Security Code").To(TestData.Organisation.SecurityCode);
            await Task.CompletedTask;
        }

        [Test, Order(26)]
        public async Task SubmittingIndentityFormLeadsToConfirmOrgDetails()
        {
            Click("Continue");
            ExpectHeader("Confirm your organisation’s details");
            await Task.CompletedTask;
        }

        [Test, Order(28)]
        public async Task ClickingBackButtonReturnsToIdentifyOrgPage()
        {
            Click("Back");
            ExpectHeader("Are you legally required to publish a modern slavery statement on your website?");
            await Task.CompletedTask;
        }
    }
}