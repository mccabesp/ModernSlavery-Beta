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

    public class Scope_Out_Confirm_Details_Content_Check : UITest
    {
        protected string EmployerReference;
        protected Organisation Org;
        [Test, Order(20)]
        public async Task AddOrgToDb()
        {
            //EmployerReference =  ModernSlavery.Testing.Helpers.Testing_Helpers.AddFastrackOrgToDB(Submission.OrgName_InterFloor, "ABCD1234");

            Org = Testing.Helpers.Extensions.OrganisationHelper.GetOrganisation(TestRunSetup.TestWebHost, Submission.OrgName_InterFloor);

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
            Set("Employer Reference").To(Org.EmployerReference);
            Set("Security Code").To(Org.SecurityCode);
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
                RightOfText("Name").Expect(Org.OrganisationName); ;
            },
                    () => { RightOfText("Reference").Expect(Org.EmployerReference); }, 
                    () => { RightOfText("Registered address").Expect(Org.LatestAddress.Address1 + ", " + Org.LatestAddress.Address2 + ", " + Org.LatestAddress.Address3 + ", " + Org.LatestAddress.TownCity + ", " + Org.LatestAddress.PostCode); },
                    () => { Expect(What.Contains, "If this information is not correct, please email"); },
                    () => { ExpectLink("modernslaverystatements@homeoffice.gov.uk"); },
                    () => { ExpectButton("Confirm and continue"); });
            await Task.CompletedTask;
        }

        [Test, Order(28)]
        public async Task VerifyLinkURL()
        {
            ExpectXPath("//a [@href='https://www.nationalarchives.gov.uk/doc/open-government-licence/version/3/' and contains(., 'testuser')]");
            await Task.CompletedTask;
        }
    }
}