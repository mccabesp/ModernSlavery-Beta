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
using ModernSlavery.Testing.Helpers.Classes;

namespace ModernSlavery.Hosts.Web.Tests
{

    [TestFixture]

    public class Scope_Out_Confirm_Details_Content_Check : BaseUITest
    {
        protected readonly OrganisationTestData TestData;
        public Scope_Out_Confirm_Details_Content_Check() : base(TestRunSetup.TestWebHost, TestRunSetup.WebDriverService)
        {
            TestData = new OrganisationTestData(this);
        }
        protected string EmployerReference;
        [OneTimeSetUp]
        public async Task SetUp()
        {
            TestData.Organisation = this.Find<Organisation>(org => TestData.Organisation.GetLatestActiveScope().ScopeStatus.IsAny(ScopeStatuses.PresumedOutOfScope, ScopeStatuses.PresumedInScope));
            //&& !o.UserOrganisations.Any(uo => uo.PINConfirmedDate != null)

            var result = this.GetSecurityCodeBusinessLogic().CreateSecurityCode(TestData.Organisation, new DateTime(2021, 6, 10));

            if (result.Failed)
            {
                throw new Exception("Unable to set security code");
            }

            await this.SaveDatabaseAsync();

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
        public async Task VerifyInfo()
        {
            //may need fixed due to missing address fields
            Try(() => {
                RightOfText("Organisation Name").Expect(TestData.Organisation.OrganisationName); ;
            },
                    () => { RightOfText("Organisation Reference").Expect(TestData.Organisation.OrganisationReference); },
                    //() => { RightOfText("Registered address").Expect(TestData.Organisation.LatestAddress.Address1); },
                    //() => { RightOfText("Registered address").Expect(TestData.Organisation.LatestAddress.Address2); },
                    //() => { RightOfText("Registered address").Expect(TestData.Organisation.LatestAddress.Address3); },
                    //() => { RightOfText("Registered address").Expect(TestData.Organisation.LatestAddress.TownCity); },
                    //() => { RightOfText("Registered address").Expect(TestData.Organisation.LatestAddress.PostCode); },
                    () => { Expect(What.Contains, "If this information is not correct, please email"); },
                    () => { ExpectLink("modernslaverystatements@homeoffice.gov.uk"); },
                    () => { ExpectButton("Confirm and continue"); });
            await Task.CompletedTask;
        }

        [Test, Order(30)]
        public async Task VerifyLinkURL()
        {
            ExpectXPath("//a [@href='https://www.nationalarchives.gov.uk/doc/open-government-licence/version/3/']");
            await Task.CompletedTask;
        }
    }
}