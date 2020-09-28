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

    [TestFixture]

    public class User_Searches_By_Org_Name_And_Filters : UITest
    {
        //Information that needs adding: 
        //Group submission with atleast 2 active organisations and a retired child organisation - Fly Jet, Fly Jet Switzerland, Fly Jet Australia (retired)
        //Organisation that has ceased trading - Pepsi & a submitted statement for company
        //Organisations - Amazon (Filters - Automotive, machinery and heavy electrical equipment, Over £500 million) , Amazing Ltd (Filters - Defence and aerospace, £60 million to £100 million), Kinder
        //Statements - Amazon statement, for year - 2019-2020
        //Adress' - Amazon: Royal Grammar School, High Street, Guildford, Surrey, GU1 3BB. Amazing Ltd: Hestercombe House, Cheddon Fitzpaine, Taunton, Somerset, TA2 8LG

        [Test, Order(1)]
        public async Task NavigateToSearchPage()
        {
            // Route to Search and compare feature to be clarified.
            ExpectHeader("Search modern slavery statements");

            await Task.CompletedTask;
        }

        [Test, Order(2)]
        public async Task SearchForOrganisationAmazon()
        {
            // Expect correct columns of information from search.
            Below("Organisation name or company number").Set("Search").To("Amazon");
            Click("Search");
            Expect("Amazon statement");
            AtRow("Amazon statement").Column("Statement year").Expect("2019 to 2020");

            await Task.CompletedTask;
        }

        [Test, Order(3)]
        public async Task ExpectRelatedResultsToSearch()
        {
            //Closely related search - (What consititues a "close" search)
            Expect("Amazing Ltd statement");
            ExpectNo("Kinder");

            await Task.CompletedTask;
        }

        [Test, Order(4)]
        public async Task ExpectResultsToBeOrderedByRelevence()
        {
            AboveRow("Amazing Ltd statement").ExpectRow("Amazon statement");

            await Task.CompletedTask;
        }

        [Test, Order(5)]
        public async Task ExpectAddressForCompanyDisplayed()
        {
            Below("Amazon statement").Expect(" Royal Grammar School, High Street, Guildford, Surrey, GU1 3BB");
            Below("Amazing ltd statement").Expect("Hestercombe House, Cheddon Fitzpaine, Taunton, Somerset, TA2 8LG");

            await Task.CompletedTask;
        }

        [Test, Order(6)]
        public async Task SearchQueryWithExpectedNoReturns()
        {
            Set("Search").To("No result");
            Click("Search");
            ExpectNo("Amazon statement");
            ExpectNo("Amazing ltd statement");
            ExpectNo("Kinder statement");
            //Expect message with prompt as to why there are no results (to be confirmed what messages).

            await Task.CompletedTask;
        }

        [Test, Order(7)]
        public async Task CeasedTradingOrganisationsStatementsStillExpected()
        {
            Set("Search").To("Pepsi");
            Click("Search");
            Expect("Pepsi statement");

            await Task.CompletedTask;
        }

        [Test, Order(7)]
        public async Task ValidationForGroupSubmissionResults()
        {
            Set("Search").To("Fly Jet");
            Click("Search");

            Expect("Fly Jet statement");
            Below("Fly Jet statement").Above("Fly Jet statement Australia").Expect("Submitted as group");

            Expect("Fly Jet Switzerland statement");
            Below("Fly Jet Australia statement").Expect("Submitted as group");

            Expect("Fly Jet Australia statement");
            Above("Fly Jet Switzerland statement").Below("Fly Jet statement").Expect("Submitted as group");

            await Task.CompletedTask;
        }
    }
}