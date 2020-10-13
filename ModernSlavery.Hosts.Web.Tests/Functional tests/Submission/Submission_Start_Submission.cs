using Geeks.Pangolin;
using ModernSlavery.Testing.Helpers.Extensions;
using NUnit.Framework;
using System.Threading.Tasks;

namespace ModernSlavery.Hosts.Web.Tests
{
    [TestFixture, Ignore("Temporary Ignore")]

    public class Submission_Start_Submission : Private_Registration_Success
    {
        

        public async Task StartSubmission()
        {
            ExpectHeader("Select an organisation");

            Click(TestData.OrgName);

            await AxeHelper.CheckAccessibilityAsync(this);
            ExpectHeader(That.Contains, "Manage your modern slavery statement submissions");

            Click("Start Draft");
            await AxeHelper.CheckAccessibilityAsync(this);

            ExpectHeader("Before you start");
            Click("Start now");
            await AxeHelper.CheckAccessibilityAsync(this);
            ExpectHeader("Your modern slavery statement");
            await Task.CompletedTask;
        }
    }
}