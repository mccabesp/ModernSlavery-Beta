using Geeks.Pangolin;
using NUnit.Framework;
using System.Threading.Tasks;

namespace ModernSlavery.Hosts.Web.Tests
{
    [TestFixture, Ignore("Temporary Ignore")]


    public class Submission_Areas_Mandatory_Field_Check : Private_Registration_Success
    {
        const string _email = Create_Account.roger_email+"1213232";
        [Test, Order(41)]
        public async Task NavigateToAreasPage()
        {
            Submission_Helper.NavigateToAreasCovered(this, Submission.OrgName_InterFloor, "2019 to 2020");
            

            await Task.CompletedTask;
        }
        [Test, Order(42)]

        public async Task OpenDetailsPopUp()
        {
            Below("Your organisation’s structure, business and supply chains").ClickLabel(The.Top, "No");
            await Task.CompletedTask;
        }

        [Test, Order(44)]
        public async Task ClickingContinueTriggersValidation()
        {
            Click("Continue");

            ExpectHeader("There is a problem");
            Expect("Missing details");

            //inline error tbc
            BelowField("Please provide details").Expect("Enter details");
        }
    }
}