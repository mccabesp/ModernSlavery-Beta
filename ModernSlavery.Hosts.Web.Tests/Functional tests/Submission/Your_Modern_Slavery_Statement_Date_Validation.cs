using Geeks.Pangolin;
using NUnit.Framework;
using System.Threading.Tasks;

namespace ModernSlavery.Hosts.Web.Tests
{
    [TestFixture, Ignore("Awaiting Submission merge")]

    public class Your_Modern_Slavery_Statement_Date_Validation : Private_Registration_Success
    {
        [Test, Order(40)]
        public async Task StartSubmission()
        {

            ExpectHeader("Select an organisation");

            Click(Submission.OrgName_Blackpool);

            ExpectHeader("Manage your organisations reporting");

            AtRow("2019/20").Click("Draft report");

            ExpectHeader("Before you start");
            Click("Start Now");

            ExpectHeader("Your modern slavery statement");
            await Task.CompletedTask;
        }
        [Test, Order(42)]
        public async Task VerifyDateToHasToBeAfterDateFrom()
        {
            //to after from
            Submission_Helper.DateSet(this, "2", "2", "2020", "1");
            Submission_Helper.DateSet(this, "2", "2", "2019", "2");

            Click("Continue");
            Expect("There is a problem");
            RefreshPage();

            await Task.CompletedTask;
        }
            [Test, Order(44)]
        public async Task VerifyDateToHasToBeInValidFormat()
        {
            //invalid format
            Submission_Helper.DateSet(this, "22", "12", "2020", "3");
            Click("Continue");
            Expect("There is a problem");
            Expect("Date format is incorrect");

            RefreshPage();
            await Task.CompletedTask;

        }
    }
}