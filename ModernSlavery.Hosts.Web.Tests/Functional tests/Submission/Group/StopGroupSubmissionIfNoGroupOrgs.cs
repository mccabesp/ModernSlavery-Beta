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
        [Test, Order(70)]
        public async Task RegisterSecondOrg()
        {
            await this.RegisterUserOrganisationAsync(Organisations[1].OrganisationName, UniqueEmail).ConfigureAwait(false);


            await Task.CompletedTask.ConfigureAwait(false);
        }
        [Test, Order(72)]
        public async Task NavigateToSubmissionPage()
        {
            Click("Your organisations");
            Click(org.OrganisationName);
            Click("Continue");
            ExpectHeader("Add your 2020 modern slavery statement to the registry");

            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(74)]
        public async Task EnsureSectionsAreCompleted()
        {
            //mandatory sections should be completed
            var MandatorySections = new string[] {
            "Organisations covered by the statement",
            "Statement URL, dates and sign-off",
            "Recommended areas covered by the statement",
            "Your organisation", //abbereviated due to ' in string
            "How many years you" //abbereviated due to ' in string
            };




            //mandaotry sections should be completed
            foreach (var mandatorySection in MandatorySections)
            {
                Submission_Helper.SectionCompleteionCheck(this, true, mandatorySection);
            }


            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(76)]
        public async Task EnsureOptionalSectionsAreIncomplete()
        {
            //all other sections incomplete 

            var OptionalSections = new string[] {
            "Policies",
            "Training",
            "Monitoring working conditions",
            "Modern slavery risks",
            "Finding indicators of modern slavery",
            "Demonstrating progress"
            };

            //optional sections incomplete
            foreach (var optionalSection in OptionalSections)
            {
                Submission_Helper.SectionCompleteionCheck(this, false, optionalSection);
            }
            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(78)]
        public async Task NavigateToConversionPage()
        {
            Click("Organisations covered by the statement");
            ExpectHeader("Does your modern slavery statement cover a single organisation or a group of organisations?");


            await Task.CompletedTask.ConfigureAwait(false);

        }

        [Test, Order(80)]
        public async Task SelectConversionToAGroupSubmission()
        {
            ClickLabel("A group of organisations");

            Click("Save and continue");
            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST").ConfigureAwait(false);

            ExpectHeader("Which organisations are included in your group statement?");

            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(82)]
        public async Task IncompleteGroup()
        {
            Click("Continue");

            await Task.CompletedTask.ConfigureAwait(false);
        }


        [Test, Order(84)]
        public async Task ButtonNotClickable()
        {
            ExpectButton("Save as draft");
            ExpectNoButton("Submit for publication");
            await Task.CompletedTask.ConfigureAwait(false);
        }

    }
}