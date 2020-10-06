﻿using Geeks.Pangolin;
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


    public class ManageOrgs_Scope_Info_Check : CreateAccount
    {
        const string _firstname = Create_Account.roger_first; const string _lastname = Create_Account.roger_last; const string _title = Create_Account.roger_job_title; const string _email = Create_Account.roger_email; const string _password = Create_Account.roger_password;

        string Pin;
        protected readonly RegistrationTestData TestData;
        public ManageOrgs_Scope_Info_Check() : base(_firstname, _lastname, _title, _email, _password)
        {
            TestData = new RegistrationTestData(this);
        }

        [Test, Order(29)]
        public async Task RegisterOrgAndSetScope()
        {

            await this.RegisterUserOrganisationAsync(TestData.Organisation.OrganisationName, UniqueEmail);
            await this.GetOrganisationBusinessLogic().CreateOrganisationSecurityCodeAsync(TestData.Organisation.OrganisationReference, new DateTime(2030, 1, 10));

            User CurrentUser = await this.ServiceScope.GetDataRepository().SingleOrDefaultAsync<User>(o => o.EmailAddress == UniqueEmail);
            await this.GetOrganisationBusinessLogic().SetAsScopeAsync(TestData.Organisation.OrganisationReference, 2020, "Updated by test case", CurrentUser, ScopeStatuses.OutOfScope, true);
        }

        [Test, Order(30)]
        public async Task GoToManageOrgPageAndSelectOrg()
        {
            Goto("/manage-organisations");

            Click("Manage organisations");
            ExpectHeader(That.Contains, "Select an organisation");

            Click(TestData.Organisation.OrganisationName);
            ExpectHeader(That.Contains, "Manage your modern slavery statement submissions");

            await Task.CompletedTask;
        }

        [Test, Order(31)]
        public async Task ExpectScopeInfoOOS()
        {

            RightOfText("2019 to 2020").BelowText("Required by law to publish a statement on your website?").Expect(What.Contains, "No");

            RightOfText("2019 to 2020").BelowText("Required by law to publish a statement on your website?").ExpectLink("Change");

            await Task.CompletedTask;
        }

        [Test, Order(32)]
        public async Task ChangeOrgStatus()
        {
            User CurrentUser = await ServiceScope.GetDataRepository().SingleOrDefaultAsync<User>(o => o.EmailAddress == UniqueEmail);
            await this.GetOrganisationBusinessLogic().SetAsScopeAsync(TestData.Organisation.OrganisationReference, 2020, "Updated by test case", CurrentUser, ScopeStatuses.InScope, true);

            await Task.CompletedTask;
        }


        [Test, Order(34)]
        public async Task ExpectScopeInfoIS()
        {
            RefreshPage();
            RightOfText("2019 to 2020").BelowText("Required by law to publish a statement on your website?").Expect(What.Contains, "Yes");

            RightOfText("2019 to 2020").BelowText("Required by law to publish a statement on your website?").ExpectLink("Change");

            await Task.CompletedTask;
        }



    }
}