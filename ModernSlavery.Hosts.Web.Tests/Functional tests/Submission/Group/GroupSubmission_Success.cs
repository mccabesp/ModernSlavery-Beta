using Geeks.Pangolin;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Testing.Helpers;
using ModernSlavery.Testing.Helpers.Extensions;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ModernSlavery.Hosts.Web.Tests
{
    [TestFixture]

    public class GroupSubmission_Success : CreateAccount
    {

        const string _firstname = Create_Account.roger_first; const string _lastname = Create_Account.roger_last; const string _title = Create_Account.roger_job_title; const string _email = Create_Account.roger_email; const string _password = Create_Account.roger_password;


        public Organisation[] Organisations { get; private set; }

        string Pin;
        public GroupSubmission_Success() : base(_firstname, _lastname, _title, _email, _password)
        {


        }
        
        protected Organisation[] organisations = new Organisation[4];
        [OneTimeSetUp]
        public async Task OTSetUp()
        {
            Organisations =   this.FindAllUnusedOrgs().ToArray();

            await Task.CompletedTask;        
        }

        [Test, Order(30)]
        public async Task RegisterOrg()
        {
            

            await this.RegisterUserOrganisationAsync(Organisations[0].OrganisationName, UniqueEmail);
            await this.RegisterUserOrganisationAsync(Organisations[1].OrganisationName, UniqueEmail);
        }

        [Test, Order(32)]
        public async Task GoToManageOrgPage()
        {
            Goto("/manage-organisations");

            Click("Manage organisations");
            ExpectHeader(That.Contains, "Select an organisation");

            await Task.CompletedTask;
        }

        [Test, Order(34)]
        public async Task SelectOrg()
        {

            Click(Organisations[0].OrganisationName);
            SubmissionHelper.MoreInformationRequiredComplete(this, true, OrgName: Organisations[0].OrganisationName);
            ExpectHeader(That.Contains, "Manage your modern slavery statement submissions");

            RightOfText("2020").BelowText("Required by law to publish a statement on your website?").Expect(What.Contains, "No");
            await Task.CompletedTask;
        }
        [Test, Order(35)]
        public async Task StartSubmission()
        {
            ExpectHeader(That.Contains, "Manage your modern slavery statement submissions");

            Click(The.Top, "Start Draft");


            ExpectHeader("Before you start");
            Click("Start now");

            ExpectHeader("Who is your statement for?");
            await Task.CompletedTask;
        }
        [Test, Order(36)]
        public async Task ChooseGroupSubmission()
        {
            ModernSlavery.Testing.Helpers.Extensions.SubmissionHelper.GroupOrSingleScreenComplete(this, true, Organisations[0].OrganisationName, "2020");

            await Task.CompletedTask;
        }

        [Test, Order(38)]
        public async Task Select4OrgsForGroup()
        {
            for (int i =1; i < 5; i++)
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

        [Test, Order(40)]
        public async Task ViewGroup()
        {
//            Expect("4  organisations included");
            Click("See which organisations you've selected");
            ExpectHeader("Review the organisations in your group statement");

            for (int i = 1; i < 5; i++)
            {
                //BelowHeader(Organisations[i].OrganisationName).Expect(Organisations[i].GetAddressString(DateTime.Now));
                Below(Organisations[i].OrganisationName).Expect("Company number: " + Organisations[i].CompanyNumber);
            }

            await Task.CompletedTask;
        }

        [Test, Order(42)]
        public async Task ContinueToSubmission()
        {
            Click("Confirm and continue");
            //ExpectHeader("Before you start");
            //Click("Start Now");
            ExpectHeader(That.Contains, "Your modern slavery statement");

            await Task.CompletedTask;
        }

        [Test, Order(43)]
        public async Task YourModernSlaveryStatement()
        {

            ExpectHeader("Your modern slavery statement");

            Set("URL").To(Submission.YourMSStatement_URL);

            Submission_Helper.DateSet(this, Submission.YourMSStatement_To_Day, Submission.YourMSStatement_To_Month, Submission.YourMSStatement_From_Year, "1");
            Submission_Helper.DateSet(this, Submission.YourMSStatement_To_Day, Submission.YourMSStatement_To_Month, Submission.YourMSStatement_To_Year, "2");

            Set("First name").To(Submission.YourMSStatement_First);
            Set("Last name").To(Submission.YourMSStatement_Last);
            Set("Job title").To(Submission.YourMSStatement_JobTitle);

            Submission_Helper.DateSet(this, Submission.YourMSStatement_ApprovalDate_Day, Submission.YourMSStatement_ApprovalDate_Month, Submission.YourMSStatement_ApprovalDate_Year, "3");

            Click("Continue");
            await Task.CompletedTask;
        }

        [Test, Order(44)]
        public async Task AreasCoveredByYourModernSlaveryStatement()
        {
            ExpectHeader("Areas covered by your modern slavery statement");

            BelowHeader("Your organisation’s structure, business and supply chains").ClickLabel(The.Top, "Yes");
            BelowHeader("Policies").ClickLabel(The.Top, "Yes");
            BelowHeader("Risk assessment and management").ClickLabel(The.Top, "Yes");
            BelowHeader("Due diligence processes").ClickLabel(The.Top, "Yes");
            BelowHeader("Staff training about slavery and human trafficking").ClickLabel(The.Top, "Yes");
            BelowHeader("Goals and key performance indicators (KPIs) to measure your progress over time, and the effectiveness of your actions").ClickLabel(The.Top, "Yes");

            Click("Continue");
            ExpectHeader(That.Contains, "Your organisation");

            await Task.CompletedTask;
        }

        [Test, Order(46)]
        public async Task NavigatePastOptionalSections()
        {

            Click("Continue");
            ExpectHeader("Policies");

            Click("Continue");
            ExpectHeader(That.Contains, "Supply chain risks and due diligence");

            Click("Continue");
            ExpectHeader(That.Contains, "Supply chain risks and due diligence");

            Click("Continue");
            ExpectHeader("Training");

            Click("Continue");
            ExpectHeader("Monitoring progress");

            Click("Continue");
            ExpectHeader("Review before submitting");

            await Task.CompletedTask;
        }

        [Test, Order(47)]
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

        [Test, Order(48)]
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
        [Test, Order(50)]
        public async Task ExitAndSaveChanges()
        {
            Click("Submit for publication");
            Expect(What.Contains, "You have submitted your modern slavery statement");
            Expect(What.Contains, "for 2020");


            await Task.CompletedTask;

        }

        [Test, Order(52)]
        public async Task CheckSubmission()
        {
            Goto("/manage-organisations");

            Click("Manage organisations");
            ExpectHeader(That.Contains, "Select an organisation");


            Click(Organisations[0].OrganisationName);
            ExpectHeader(That.Contains, "Manage your modern slavery statement submissions");

            DateTime now = DateTime.Now;

            RightOfText("2020").BelowText("Status of statement published on this service").Expect(What.Contains, "Published");


                RightOfText("2020").BelowText("Status of statement published on this service").Expect(What.Contains, "on " + now.Day + " " + now.ToString("MMMM") + " " + now.Year );


            await Task.CompletedTask;

        }

        [Test, Order(54)]
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