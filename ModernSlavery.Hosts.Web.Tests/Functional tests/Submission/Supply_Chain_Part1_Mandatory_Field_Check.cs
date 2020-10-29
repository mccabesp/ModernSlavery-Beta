using Geeks.Pangolin;
using Geeks.Pangolin.Helper.UIContext;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Testing.Helpers.Extensions;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;

namespace ModernSlavery.Hosts.Web.Tests
{
    [TestFixture, Ignore("Awaiting Submission merge")]

    public class Supply_Chain_Part1_Mandatory_Field_Check : CreateAccount
    {
        const string _firstname = Create_Account.roger_first; const string _lastname = Create_Account.roger_last; const string _title = Create_Account.roger_job_title; const string _email = Create_Account.roger_email; const string _password = Create_Account.roger_password;


        protected Organisation org;
        [OneTimeSetUp]
        public async Task SetUp()
        {
            org = this.Find<Organisation>(org => org.GetLatestActiveScope().ScopeStatus.IsAny(ScopeStatuses.PresumedOutOfScope, ScopeStatuses.PresumedInScope) && org.LatestRegistrationUserId == null && !org.UserOrganisations.Any());


        }

        public Supply_Chain_Part1_Mandatory_Field_Check() : base(_firstname, _lastname, _title, _email, _password)
        {


        }

        [Test, Order(29)]
        public async Task RegisterOrg()
        {
            await this.RegisterUserOrganisationAsync(org.OrganisationName, UniqueEmail);
            RefreshPage();

            await Task.CompletedTask;
        }
        [Test
            , Order(40)]
        public async Task NavigateToSCRPart1()
        {
            Submission_Helper.NavigateToSupplyChainRisks1(this, org.OrganisationName, "2020", MoreInfoRequired: true);
            await AxeHelper.CheckAccessibilityAsync(this);
            await Task.CompletedTask;
        }

        [Test, Order(42)]
        public async Task TriggerAllOtherFieldsToAppear()
        {
            //only mandatory fields are other fields
            ClickText(The.Top, That.Contains, "Open all");
            //trigger all other fields by clicking all "other" options
            string[] OtherOptions = new string[] { "Other vulnerable group(s)", "Other type of work", "Other sector" };

            foreach (var Option in OtherOptions)
            ClickLabel(Option);

            await Task.CompletedTask;
        }

        [Test, Order(44)]
        public async Task SubmitFormWithoutFillingInOptionDetails()
        {
            Click("Continue");
            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST");
            await Task.CompletedTask;
        }

        [Test, Order(46)]
        public async Task CheckValidationMessages()
        {
            Expect("There is a problem");
            Expect("Missing details");
            Expect("Enter details");
            await Task.CompletedTask;
        }

        [Test, Order(48)]
        public async Task FillInMandatoryFields()
        {
            //all fields must be filled
            var OtherFields = new string[] { "RelevantRisks[8].Details", "RelevantRisks[13].Details", "RelevantRisks[22].Details" };
            foreach (var  Field in OtherFields)
            {
                Set(Field).To("Details");
                Click("Continue");

                Expect("There is a problem");
                Expect("Missing details");
            }

            //set last field
            Set("If you want to specify an area not mentioned above, please provide details").To("Details");
            CopyUrl("Part 1");

            await Task.CompletedTask;
        }
        [Test, Order(49)]
        public async Task ClickingCOntinueGoesToPart2()
        {
            //expect new page

            Click("Continue");
            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST");
            Expect("Part 2");
            await Task.CompletedTask;
        }

        [Test, Order(50)]
        public async Task ReturnToPart1()
        {
            GotoCopiedUrl("Part 1");
            await AxeHelper.CheckAccessibilityAsync(this);
            Expect("Part 1");
            await Task.CompletedTask;
        }

        [Test, Order(52)]
        public async Task PleaseExplainWhyChecks()
        {
            //when selecting option additional mandatory field appears
            PleaseExplainWhyCheck(this, Submission.GoodsAndServices, "Part 1");
            PleaseExplainWhyCheck(this, Submission.VulnerableGroups, "Part 1");
            PleaseExplainWhyCheck(this, Submission.TypesOfWork, "Part 1");
            PleaseExplainWhyCheck(this, Submission.Sectors, "Part 1");
            PleaseExplainWhyCheck(this, Submission.GoodsAndServices, "Part 1");
            await Task.CompletedTask;
        }
    

        private static void PleaseExplainWhyCheck(UIContext ui, string[] options, string URL) 
        {
            foreach (var Option  in options)
            {
                ui.BelowLabel(Option).ExpectNoField("Option");
                ui.ClickLabel(Option);
                ui.BelowLabel(Option).ExpectField("Please explain why");
                ui.Click("Continue");
                ui.Expect("There is a problem");

                ui.BelowLabel(Option).Set(The.Top, "Please explain why").To("Reason");
                ui.Click("Continue");
                ui.Expect("Part 2");
                ui.GotoCopiedUrl(URL);
            }
        }

    }
}