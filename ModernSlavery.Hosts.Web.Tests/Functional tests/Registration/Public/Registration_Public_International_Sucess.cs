using Geeks.Pangolin;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Testing.Helpers.Extensions;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ModernSlavery.Hosts.Web.Tests
{
    [TestFixture]

    public class Registration_Public_International_Success : Registration_Public_Start_Registration
    {
        const string _firstname = Create_Account.roger_first; const string _lastname = Create_Account.roger_last; const string _title = Create_Account.roger_job_title; const string _email = Create_Account.roger_email; const string _password = Create_Account.roger_password;

        protected Organisation org;
        [OneTimeSetUp]
        public async Task SetUp()
        {
            org = this.Find<Organisation>(org => org.SectorType.IsAny(SectorTypes.Public) && !org.UserOrganisations.Any()); ;


            org = this.Find<Organisation>(org => org.SectorType.IsAny(SectorTypes.Public) && org.GetLatestActiveScope().ScopeStatus.IsAny(ScopeStatuses.PresumedOutOfScope, ScopeStatuses.PresumedInScope) && org.LatestRegistrationUserId == null && !org.UserOrganisations.Any());
        }

        [Test, Order(30)]
        public async Task ExpectAddressFields()
        {


            //            ClickButton(That.Contains, "Choose");
            ExpectHeader("Your organisation's address");

            var OrgAdress = org.GetAddress(DateTime.Now);
            //fields pre-populated
            //todo discuss address issue - RegistrationController.Manual
            //AtField("Address 1").Expect(OrgAdress.Address1);
            //AtField("Address 2").Expect(OrgAdress.TownCity);
            //AtField("Address 3").Expect(OrgAdress.County);
            //AtField("Postcode").Expect(OrgAdress.PostCode);
            await Task.CompletedTask.ConfigureAwait(false);
        }
        [Test, Order(31)]
        public async Task ClickingContinueNavigatesToContactDetailsPage()
        {
            Click("Continue");
            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST").ConfigureAwait(false);

            ExpectHeader("Your contact details");
            ExpectText("Enter your contact details.");
            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(32)]
        public async Task ExpectContactDetailsFields()
        {
            //fields pre-populated
            AtField("First name").Expect(Create_Account.roger_first);
            AtField("Last name").Expect(Create_Account.roger_last);
            AtField("Email address").Expect(UniqueEmail);
            AtField("Job title").Expect(Create_Account.roger_job_title);
            Set("Telephone number").To("01414453344");
            await Task.CompletedTask.ConfigureAwait(false);

        }

        [Test, Order(33)]
        public async Task ClickingContinueNavigatesToOrgDetailsPage()
        {
            Click("Continue");
            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST").ConfigureAwait(false);

            ExpectHeader("Confirm your organisation’s details");
            await Task.CompletedTask.ConfigureAwait(false);

        }

        [Test, Order(34)]
        public async Task ExpectOrgDetailsFields()
        {

            RightOfText("Organisation name").Expect(org.OrganisationName);
            //RightOfText("Registered address").Expect(org.GetAddressString(DateTime.Now));

            ExpectRow("Your contact details");
            RightOfText("Your name").Expect(Create_Account.roger_first + " " + Create_Account.roger_last + " (" + Create_Account.roger_job_title + ")");
            RightOfText("Email").Expect(UniqueEmail);
            RightOfText("Telephone").Expect("01414453344");
            await Task.CompletedTask.ConfigureAwait(false);

        }
        [Test, Order(36)]
        public async Task ConfirmNonUkAddress()
        {
            ExpectHeader("Is this a UK address?");
            ClickLabel("No");
            //should take you to the international workflow
            Click("Confirm");


            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST").ConfigureAwait(false);

            await Task.CompletedTask.ConfigureAwait(false);

            //todo await confirmation of international workflow
        }
    }
}