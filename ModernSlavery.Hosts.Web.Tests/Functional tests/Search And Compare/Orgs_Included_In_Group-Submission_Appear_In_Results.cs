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
using ModernSlavery.Testing.Helpers.Classes;
using ModernSlavery.Testing.Helpers.Extensions;

namespace ModernSlavery.Hosts.Web.Tests
{

    [TestFixture, Ignore("Being fixed on other branch")]

    public class Orgs_Included_In_Group_Submission_Appear_In_Results : GroupSubmission_Success
    {
            
           [Test, Order(60)]
    public async Task ReturnToRoot()
    {
            SignOutDeleteCookiesAndReturnToRoot(this);
            ExpectHeader("See what organisations are doing to tackle modern slavery");

            await Task.CompletedTask;
    }

    [Test, Order(62)]
    public async Task Navigate_To_Search()
    {
            Click("Find statements");

            ExpectHeader("Search modern slavery statements");
            await Task.CompletedTask;
    }

    [Test, Order(64)]
    public async Task Find_Group_Statements()
    {
            SetXPath("//input[@class='gov-uk-c-searchbar__input']").To(Organisations[0].OrganisationName);

            ClickXPath("//button[@class='gov-uk-c-searchbar__submit gov-uk-c-searchbar__submit--blue']");


            Expect("1 result");
        
            
            await Task.CompletedTask;
    }

        [Test, Order(66)]
        public async Task CheckParentOrg()
        {
            AtXPath("//p[contains(., '" + Organisations[0].OrganisationName + "')]//parent::div").Expect("2020");
            AtXPath("//p[contains(., '" + Organisations[0].OrganisationName + "')]//parent::div").Expect(What.Contains, "Reported as group");
            AtXPath("//p[contains(., '" + Organisations[0].OrganisationName + "')]//parent::div").Expect("Company number: " + Organisations[0].CompanyNumber);                           
            AtXPath("//p[contains(., '" + Organisations[0].OrganisationName + "')]//parent::div").Expect("Company number: " + Organisations[0].GetAddressString(DateTime.Now));


            await Task.CompletedTask;
        }
        [Test, Order(68)]
    public async Task How_Can_We_Improve()
    {
            Click("Back");
            Click("See all statements");
            Expect("5 results");

            for (int i = 1; i < 5; i++)
            {
                

                
                AtXPath("//p[contains(., '" + Organisations[i].OrganisationName + "')]//parent::div").Expect("2020");
                AtXPath("//p[contains(., '" + Organisations[i].OrganisationName + "')]//parent::div").ExpectXPath("//span[contains(., 'Submitted under')]//parent::p//span[contains(., '"+ Organisations[0].OrganisationName + "')]");
                AtXPath("//p[contains(., '" + Organisations[i].OrganisationName + "')]//parent::div").Expect("Company number: " + Organisations[0].CompanyNumber);
                AtXPath("//p[contains(., '" + Organisations[i].OrganisationName + "')]//parent::div").Expect("Company number: " + Organisations[0].GetAddressString(DateTime.Now)) ;


            }
            await Task.CompletedTask;
    }
    
}
}