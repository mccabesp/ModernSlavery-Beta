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
    [TestFixture, Ignore("Bug raised in 3112, test case to be fixed and broken down once resolved")]
    public class Registration_Public_Cant_Find_Your_Org_Success : CreateAccount
    {
        const string _firstname = Create_Account.roger_first; const string _lastname = Create_Account.roger_last; const string _title = Create_Account.roger_job_title; const string _email = Create_Account.roger_email; const string _password = Create_Account.roger_password;

        public Registration_Public_Cant_Find_Your_Org_Success() : base(_firstname, _lastname, _title, _email, _password)
        {
        }
        [Test, Order(20)]
        public async Task Registration_Public_Cant_Find_Your_Org_Success2()
        {

            ClickText("Register an organisation");

            ExpectHeader("Registration Options");

            ClickLabel("Public Sector Organisation");

            Click("Continue");

            ExpectHeader("Find your organisation");

            SetXPath("//input[@id='SearchText']").To(Registration.OrgName_Blackpool);
            Click(The.Bottom, "Search");


            Expect("0 employers found that match your search");

            //message should not appear with single result 
            ExpectNo(What.Contains, "Showing 1-");

            Click("Can't find your organisation?");
            Click("Tell us about your organisation");

            ExpectHeader(That.Contains, "Details of the organisation you're");

            //org name pre-filled by search
            AtField("Organisation name").Expect(Registration.OrgName_Blackpool);


            Expect("Enter one or more unique references to help identify your organisation:");

            Set("Company number").To("12345678");

            Click("Other reference?");
            Set("Name or type (e.g., 'DUNS ')").To("DUNS");
            Set("Unique number or value (e.g., '987654321')").To("01233345555");
            Click("Continue");
            ExpectHeader("Select your organisation");
        }
    }
}