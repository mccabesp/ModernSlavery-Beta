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

    public class GroupSubmission_SwitchSingleToGroup : Submission_Complete_Mandatory_Sections

        //Information needing to be added: A second variant organisation to add to grouping Fly Jet Australia
    {
        //[OneTimeSetUp]
        //public async Task OTSetUp()
        //{
        //    TestData.Organisation = TestRunSetup.TestWebHost
        //        .Find<Organisation>(org => org.GetLatestActiveScope().ScopeStatus.IsAny(ScopeStatuses.PresumedOutOfScope, ScopeStatuses.PresumedInScope) && o.LatestRegistrationUserId == null));
        //    //&& !o.UserOrganisations.Any(uo => uo.PINConfirmedDate != null)
        //}
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
        public async Task AddAndConfirmGroup()
        {
            Click(What.Contains, "tell us which organisations are included in your statement");
            Click(What.Contains, "Select more organisations");

            
                for (int i = 1; i < 5; i++)
                {
                    SetXPath("//input[@class = 'gov-uk-c-searchbar__input']").To(Organisations[i].OrganisationName);
                    Click("Search");
                    Below(What.Contains, "Can't find the organisation you're looking for?").Expect(Organisations[i].OrganisationName);
                    Below(What.Contains, "Can't find the organisation you're looking for?").RightOf(Organisations[i].OrganisationName).Expect(Organisations[i].CompanyNumber);
                    Below(What.Contains, "Can't find the organisation you're looking for?").RightOf(Organisations[i].OrganisationName).Click(The.Top, "Include");
                    //Expect(i + "  organisations included");
                }

                await Task.CompletedTask;
            }

            [Test, Order(61)]
            public async Task ViewGroup()
            {
                    Click("View your group");
                //            Expect("4  organisations included");
                ExpectHeader("Review the organisations in your group statement");

                for (int i = 1; i < 5; i++)
                {
                    //BelowHeader(Organisations[i].OrganisationName).Expect(Organisations[i].GetAddressString(DateTime.Now));
                    Below(Organisations[i].OrganisationName).Expect("Company number: " + Organisations[i].CompanyNumber);
                }

                await Task.CompletedTask;
            }

            [Test, Order(62)]
            public async Task ContinueToSubmission()
            {
                Click("Confirm and continue");
                ExpectHeader("Review before submitting");

                await Task.CompletedTask;
            }

            

            [Test, Order(70)]
            public async Task OptionalSectionsIncomplete()
            {
                //mandaotry sections should be completed
                RightOf("Your modern Slavery statement").Expect("Completed");
                RightOf("Areas covered by your modern statement").Expect("Completed");

                //optional sections incomplete
                RightOf("Your organisation").Expect("Not Completed");
                RightOf("Policies").Expect("Not Completed");
                RightOf("Supply chain risks and due diligence (part 1)").Expect("Not Completed");
                RightOf("Supply chain risks and due diligence (part 2)").Expect("Not Completed");
                RightOf("Training").Expect("Not Completed");
                RightOf("Monitoring progress").Expect("Not Completed");
                await Task.CompletedTask;
            }

            [Test, Order(72)]
            public async Task GroupReviewPage()
            {
                //Expect("2020 modern slavery statement for "+ Organisations[0].OrganisationName + " (group)");

                Expect("2020 modern slavery statement for " + Organisations[0].OrganisationName);

                Expect(What.Contains, "You can ");
                ExpectLink(That.Contains, "review and edit the organisations");
                Expect(What.Contains, " included in this group statement,");
                Expect(What.Contains, "or");
                ExpectLink(That.Contains, "tell us it's a single organisation");
                Expect(What.Contains, " instead.");
                await Task.CompletedTask;
            }
            [Test, Order(74)]
            public async Task ExitAndSaveChanges()
            {
                Click("Submit for publication");
                Expect(What.Contains, "You have submitted your modern slavery statement");
                Expect(What.Contains, "for 2020");


                await Task.CompletedTask;

            }

            [Test, Order(76)]
            public async Task CheckSubmission()
            {
                Goto("/manage-organisations");

                Click("Manage organisations");
                ExpectHeader(That.Contains, "Select an organisation");


                Click(Organisations[0].OrganisationName);
                ExpectHeader(That.Contains, "Manage your modern slavery statement submissions");

                DateTime now = DateTime.Now;

                RightOfText("2020").BelowText("Status of statement published on this service").Expect(What.Contains, "Published");


                RightOfText("2020").BelowText("Status of statement published on this service").Expect(What.Contains, "on " + now.Day + " " + now.ToString("MMMM") + " " + now.Year);


                await Task.CompletedTask;

            }

            [Test, Order(78)]
            public async Task CheckGroupeeSubmissionStatus()
            {
                Goto("/manage-organisations");

                Click("Manage organisations");
                ExpectHeader(That.Contains, "Select an organisation");


                Click(Organisations[1].OrganisationName);
                SubmissionHelper.MoreInformationRequiredComplete(this, true, OrgName: Organisations[1].OrganisationName);
                ExpectHeader(That.Contains, "Manage your modern slavery statement submissions");

                //RightOfText("2020").BelowText("Status of statement published on this service").Expect(What.Contains, "Already included in "+ Organisations[0].OrganisationName + "’s 2020 group submission, published on " + DateTime.Now.ToString("dd MMM yyyy"));

                AtXPath("(//div[@class = 'gpg-manage-reports__cell gpg-manage-reports__cell--year' and contains(., '2020')][1]//parent::div)[1]").Expect(What.Contains, "Already included in " + Organisations[0].OrganisationName + "'s 2020 group submission, published on " + DateTime.Now.ToString("d MMM yyyy"));

                AtXPath("(//div[@class = 'gpg-manage-reports__cell gpg-manage-reports__cell--year' and contains(., '2019')][1]//parent::div)[1]").Expect(What.Contains, "Not Started");
                await Task.CompletedTask;
            }
        }
}