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

    public class StopGroupSubmissionIfNoGroupOrgs : Submission_Complete_Mandatory_Sections

    {
        
        public Organisation[] Organisations { get; private set; }

        [OneTimeSetUp]
        public async Task SetUp()
        {
            

            Organisations = this.FindAllUnusedOrgs().ToArray();

            org = Organisations[0];

        }
        [Test, Order(50)]
        public async Task RegisterSecondOrg()
        {
            await this.RegisterUserOrganisationAsync(Organisations[1].OrganisationName, UniqueEmail);


            await Task.CompletedTask;
        }
        [Test, Order(51)]
        public async Task NavigateToSubmissionPage()
        {
            Click("Manage organisations");
            Click(org.OrganisationName);
            Click("Continue");
            ExpectHeader("Review before submitting");

            await Task.CompletedTask;
        }

        [Test, Order(52)]
        public async Task EnsureSectionsAreCompleted()
        {
            //mandatory sections should be completed
            RightOfText("Your modern slavery statement").ExpectText("Completed");
            RightOfText("Areas covered by your modern statement").ExpectText("Completed");
            await Task.CompletedTask;
        }

        [Test, Order(54)]
        public async Task EnsureOptionalSectionsAreIncomplete()
        {
            //all other sections incomplete 

            RightOfText("Your organisation").ExpectText("Not Completed");
            RightOfText("Policies").ExpectText("Not Completed");
            RightOfText("Supply chain risks and due diligence (part 1)").ExpectText("Not Completed");
            RightOfText("Supply chain risks and due diligence (part 2)").ExpectText("Not Completed");
            RightOfText("Training").ExpectText("Not Completed");
            RightOfText("Monitoring progress").ExpectText("Not Completed");
            await Task.CompletedTask;
        }

        [Test, Order(56)]
        public async Task NavigateToConversionPage()
        {
            Expect("Is this statement for a group of organisations?");
            ClickText("Is this statement for a group of organisations?");

            ExpectText(That.Contains, "If your statement is for a group of organisations, you need to ");
            Expect(What.Contains, "specify it’s for a group");
            ExpectText(That.Contains, "and tell us which organisations are in the group.");
            ClickText(That.Contains, "specify it’s for a group");
            ExpectHeader("Who is your statement for?");

            await Task.CompletedTask;
        }

        [Test, Order(57)]
        public async Task SelectConversionToAGroupSubmission()
        {
            //Expect("a single organisation");
            //Expect("a group of organisations");
            //Click("a group of organisations");

            ExpectHeader("The 2020 modern slavery statement for " + org.OrganisationName + " covers:");
        
                ClickLabel("a group of organisations");
                Click("Continue");

            await Task.CompletedTask;
        }

        [Test, Order(58)]
        public async Task IncompleteGroup()
        {
            Expect("We need more information");
            Expect(What.Contains, "tell us which organisations are included in your statement");
           
                await Task.CompletedTask;
            }
           

            [Test, Order(62)]
            public async Task ButtonNotClickable()
            {
            ExpectXPath("//button[@disabled = 'disabled' and contains(text(), 'Submit')]");
                await Task.CompletedTask;
            }
                 
        }
}