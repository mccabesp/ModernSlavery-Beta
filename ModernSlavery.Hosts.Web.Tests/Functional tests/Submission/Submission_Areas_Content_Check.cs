using Geeks.Pangolin;
using NUnit.Framework;
using System.Threading.Tasks;

namespace ModernSlavery.Hosts.Web.Tests
{
    [TestFixture, Ignore("Awaiting Submission merge")]

    public class Submission_Areas_Content_Check : Private_Registration_Success
    {
        [Test, Order(40)]
        public async Task StartSubmission()
        {
            ExpectHeader("Select an organisation");

            Click(Submission.OrgName_Blackpool);


            ExpectHeader("Manage your organisations reporting");

            Click("Draft Report");


            ExpectHeader("Before you start");
            Click("Start now");

            ExpectHeader("Your modern slavery statement");
            await Task.CompletedTask;
        }

        [Test, Order(42)]
        public async Task ContinueLeadsToAreasPage()
        {
            Click("Save and continue");

            ExpectHeader("Areas covered by your modern slavery statement");
            ExpectText("Does your modern slavery statement cover the following areas in relation to slavery and human trafficking?");

            await Task.CompletedTask;
        }
        [Test, Order(44)]
        public async Task VerifyAreaPageContent()
        {
            AtLabel("Your organisation’s structure, business and supply chains").ExpectLabel("Yes");
            AtLabel("Your organisation’s structure, business and supply chains").ExpectLabel("No");
            //field should now appears
            AtLabel("Your organisation’s structure, business and supply chains").ClickLabel("Yes");
            ExpectField("Please provide details");

            AtLabel("Policies").ExpectLabel("Yes");
            AtLabel("Policies").ExpectLabel("No");
            AtLabel("Risk assessment and management").ExpectLabel("Yes");
            AtLabel("Risk assessment and management").ExpectLabel("No");
            AtLabel("4. The areas of your business and supply chains where there is a risk of slavery and human trafficking taking place, and the steps you have taken to assess and manage that risk?").ExpectLabel("Yes");
            AtLabel("4. The areas of your business and supply chains where there is a risk of slavery and human trafficking taking place, and the steps you have taken to assess and manage that risk?").ExpectLabel("No");
            AtLabel("Due diligence processes").ExpectLabel("Yes");
            AtLabel("Due diligence processes").ExpectLabel("No"); 
            AtLabel("Staff training about slavery and human trafficking").ExpectLabel("Yes");
            AtLabel("Staff training about slavery and human trafficking").ExpectLabel("No");
            AtLabel("Goals and key performance indicators (KPIs) to measure your progress over time, and the effectiveness of your actions").ExpectLabel("Yes");
            AtLabel("Goals and key performance indicators (KPIs) to measure your progress over time, and the effectiveness of your actions").ExpectLabel("No");

            ExpectButton("Save and continue");
            ExpectButton("Cancel");
            await Task.CompletedTask;

        }
    }
}