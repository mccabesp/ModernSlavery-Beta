using Geeks.Pangolin;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Testing.Helpers.Extensions;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;

namespace ModernSlavery.Hosts.Web.Tests
{
    [TestFixture]
    public class Submission_Areas_Date_Validation : CreateAccount
    {
        const string _firstname = Create_Account.roger_first; const string _lastname = Create_Account.roger_last; const string _title = Create_Account.roger_job_title; const string _email = Create_Account.roger_email; const string _password = Create_Account.roger_password;

        private Organisation org;
        [OneTimeSetUp]
        public async Task SetUp()
        {
            //HostHelper.ResetDbScope();
        }

        public Submission_Areas_Date_Validation() : base(_firstname, _lastname, _title, _email, _password)
        {
        }

        [Test, Order(29)]
        public async Task RegisterOrg()
        {
            org = this.Find<Organisation>(org => org.GetLatestActiveScope().ScopeStatus.IsAny(ScopeStatuses.PresumedOutOfScope, ScopeStatuses.PresumedInScope) && org.LatestRegistrationUserId == null && !org.UserOrganisations.Any());

            await this.RegisterUserOrganisationAsync(org.OrganisationName, UniqueEmail).ConfigureAwait(false);
            RefreshPage();

            await this.SaveDatabaseAsync().ConfigureAwait(false);

            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(41)]
        public async Task NavigateToAreasPage()
        {
            ExpectHeader("Register or select organisations you want to add statements for");

            Click(org.OrganisationName);
            SubmissionHelper.MoreInformationRequiredComplete(this, true, OrgName: org.OrganisationName);
            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST").ConfigureAwait(false);
            ExpectHeader(That.Contains, "Manage your modern slavery statements");


            Click(The.Top, "Start Draft");
            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST").ConfigureAwait(false);
            ExpectHeader("Before you start");


            Click("Continue");
            await AxeHelper.CheckAccessibilityAsync(this).ConfigureAwait(false);
            ExpectHeader("Transparency and modern slavery");

            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST").ConfigureAwait(false);

            Click("Continue");
            await AxeHelper.CheckAccessibilityAsync(this).ConfigureAwait(false);
            ExpectHeader("Add your 2020 modern slavery statement to the registry");


            Click("Organisations covered by the statement");
            ModernSlavery.Testing.Helpers.Extensions.SubmissionHelper.GroupOrSingleScreenComplete(this, OrgName: TestData.OrgName);

            Click("Statement URL, dates and sign-off");
            ExpectHeader("Provide a link to the modern slavery statement on your organisation's website");
            Set("URL").To(Submission.YourMSStatement_URL);

            Click("Save and Continue");
            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(44)]
        public async Task CheckDateMustBeValidDate()
        {
            Submission_Helper.DateSet(this, "31", "02", "2019", "1");
            Click("Save and continue");
            Expect("Invalid start date");
            Expect("Start date must be a valid date");

            Submission_Helper.DateSet(this, "31", "02", "2020", "2");
            Click("Save and continue");
            Expect("Invalid end date");
            Expect("End date must be a valid date");

            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(45)]
        public async Task CheckDatesAreWithinTimeFrame()
        {
            //past from date
            Submission_Helper.DateSet(this, "21", "02", "2021", "1");
            Click("Save and continue");
            Expect("Invalid start date");
            Expect("From date must be in the past");
            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(46)]
        public async Task CheckFromDateIsBeforeToDate()
        {
            //from date before to date
            Submission_Helper.DateSet(this, "20", "02", "2020", "1");
            Submission_Helper.DateSet(this, "10", "02", "2020", "2");
            Click("Save and continue");
            Expect("Invalid date range");
            Expect("The from date must be before the to date");
            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(47)]
        public async Task FromDateMustBeBeforeToDate()
        {
            //same date
            Submission_Helper.DateSet(this, "20", "02", "2020", "2");
            Click("Save and continue");
            Expect("Invalid date range");
            Expect("The from date must be before the to date");
            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(48)]
        public async Task WithinYearlyRange()
        {
            //out of year range
            Submission_Helper.DateSet(this, "20", "02", "2021", "2");
            Click("Save and continue");
            // Expect("Invalid date range");
            Expect("Year must be between 2019 and 2020");
            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(49)]
        public async Task TwelveTo24MonthDateRanges()
        {
            // between 12 and 18 month range
            Submission_Helper.DateSet(this, "20", "02", "2018", "1");
            Submission_Helper.DateSet(this, "21", "02", "2018", "2");
            Click("Save and continue");
            Expect("Invalid end date");
            Expect("Invalid date range");
            Expect("The period between from and to dates must be between 12 and 18 months");

            // between 12 and 18 month range
            Submission_Helper.DateSet(this, "20", "02", "2018", "1");
            Submission_Helper.DateSet(this, "21", "08", "19", "2");
            Click("Save and continue");
            Expect("Invalid end date");
            Expect("Invalid date range");
            Expect("The period between from and to dates must be between 12 and 18 months");
            Expect("To date must be in the past");
            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}