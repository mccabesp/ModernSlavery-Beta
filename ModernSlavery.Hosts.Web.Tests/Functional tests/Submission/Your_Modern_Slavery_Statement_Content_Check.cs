using Geeks.Pangolin;
using ModernSlavery.Testing.Helpers.Extensions;
using NUnit.Framework;
using System.Threading.Tasks;

namespace ModernSlavery.Hosts.Web.Tests
{
    [TestFixture, Ignore("Awaiting Submission merge")]

    public class Your_Modern_Slavery_Statement_Content_Check : Private_Registration_Success
    {
        [Test, Order(40)]
        public async Task StartSubmission()
        {
            ExpectHeader("Select an organisation");

            Click(Submission.OrgName_Blackpool);
            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST");

            ExpectHeader("Manage your organisations reporting");

            AtRow("2019/20").Click("Draft report");
            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST");

            ExpectHeader("Before you start");
            Click("Start Now");

            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST");
            ExpectHeader("Your modern slavery statement");
            await Task.CompletedTask;
        }

        [Test, Order(42)]
        public async Task VerifyContent()
        {
            Expect("Provide a link to the modern slavery statement on your organisation's website");

            ExpectLink("More information");

            ExpectField("Url");

            //todo confirm format of day month year fields
            AtLabel(The.Top, "Date").ExpectField("Day");
            AtLabel(The.Top, "Date").ExpectField("Month");
            AtLabel(The.Top, "Date").ExpectField("Year");

            AtLabel("to").ExpectField("Day");
            AtLabel("to").ExpectField("Month");
            AtLabel("to").ExpectField("Year");

            Expect("What is the name of the director (or equivalent) that signed off your statement?");

            ExpectField("First name");
            ExpectField("Last name");
            ExpectField("Job title");

            ExpectHeader("What date was your statement approved by the board or equivalent management body?");

            AtLabel(The.Bottom, "Date").ExpectField("Day");
            AtLabel(The.Bottom, "Date").ExpectField("Month");
            AtLabel(The.Bottom, "Date").ExpectField("Year");


            ExpectButton("Save and continue");

            ExpectButton("Cancel");
            await Task.CompletedTask;

        }
    }
}