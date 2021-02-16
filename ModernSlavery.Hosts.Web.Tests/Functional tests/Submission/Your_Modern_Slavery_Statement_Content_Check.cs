using Geeks.Pangolin;
using ModernSlavery.Testing.Helpers.Extensions;
using NUnit.Framework;
using System.Threading.Tasks;

namespace ModernSlavery.Hosts.Web.Tests
{
    [TestFixture]

    public class Your_Modern_Slavery_Statement_Content_Check : Private_Registration_Success
    {
        [Test, Order(40)]
        public async Task StartSubmission()
        {
            Goto("/manage-organisations");
            ExpectHeader("Register or select organisations you want to add statements for");

            Click(Submission.OrgName_Blackpool);
            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST").ConfigureAwait(false);

            ExpectHeader(That.Contains, "Manage your modern slavery statements");

            AtRow("2019/20").Click("Start draft");
            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST").ConfigureAwait(false);

            ExpectHeader("Before you start");
            Click("Continue");

            ExpectHeader("Transparency and modern slavery");
            Click("Continue");

            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST").ConfigureAwait(false);
            ExpectHeader("Add your 2020 modern slavery statement to the registry");
            await Task.CompletedTask.ConfigureAwait(false);
        }

    }
}