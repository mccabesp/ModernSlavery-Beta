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

            await this.RegisterUserOrganisationAsync(org.OrganisationName, UniqueEmail);
            RefreshPage();

            await this.SaveDatabaseAsync();

            await Task.CompletedTask;
        }
       
        [Test, Order(41)]
        public async Task NavigateToAreasPage()
        {
            Submission_Helper.NavigateYourMSStatement(this, org.OrganisationName, "2020", MoreInfoRequired: true);
            await AxeHelper.CheckAccessibilityAsync(this);

            await Task.CompletedTask;
        }
        
        [Test, Order(44)]
        public async Task CheckDateMustBeValidDate()
        {           

            Submission_Helper.DateSet(this, "31", "02", "2015", "1");
            Click("Continue");
            Expect("Invalid start date");
            Expect("Start date must be a valid date");

            Submission_Helper.DateSet(this, "31", "02", "2015", "2");
            Click("Continue");
            Expect("Invalid end date");
            Expect("End date must be a valid date");

            await Task.CompletedTask;

        }

        [Test, Order(45)]
        public async Task CheckDatesAreWithinTimeFrame()
        {
            //past from date
            Submission_Helper.DateSet(this, "21", "02", "2021", "1");
            Click("Continue");
            Expect("Invalid start date");
            Expect("From date must be in the past");
            await Task.CompletedTask;

        }

       [Test, Order(46)]
         public async Task CheckFromDateIsBeforeToDate()
            {
                //from date before to date
                Submission_Helper.DateSet(this, "20", "02", "2020", "1");
                Submission_Helper.DateSet(this, "10", "02", "2020", "2");
                Click("Continue");
                Expect("Invalid date range");
                Expect("The from date must be before the to date");
                await Task.CompletedTask;

        }

        [Test, Order(47)]
        public async Task FromDateMustBeBeforeToDate()
        {

            //same date
            Submission_Helper.DateSet(this, "20", "02", "2020", "2");
            Click("Continue");
            Expect("Invalid date range");
            Expect("The from date must be before the to date");
            await Task.CompletedTask;

        }

        [Test, Order(48)]
        public async Task WithinYearlyRange()
        {
            //out of year range
            Submission_Helper.DateSet(this, "20", "02", "2021", "2");
            Click("Continue");
           // Expect("Invalid date range");
            Expect("Year must be between 2019 and 2020");
            await Task.CompletedTask;

        }

        [Test, Order(49)]
        public async Task TwelveTo24MonthDateRanges()
        {

            // between 12 and 24 month range
            Submission_Helper.DateSet(this, "20", "02", "2018", "1");
            Submission_Helper.DateSet(this, "21", "02", "2018", "2");
            Click("Continue");
            Expect("Invalid end date");
            Expect("Invalid date range");
            Expect("The period between from and to dates must be a minimum of 12 months and a max of 24 months");

            await Task.CompletedTask;

        }
        
    }
}