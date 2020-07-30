using Geeks.Pangolin;
using Geeks.Pangolin.Helper.UIContext;
using NUnit.Framework;
using System.Threading.Tasks;

namespace ModernSlavery.Hosts.Web.Tests
{
    [TestFixture, Ignore("Waiting for submission merge")]

    public class Review_And_Edit_Link_Navigation : Registration_Public_Success
    {
        const string _firstname = Create_Account.roger_first; const string _lastname = Create_Account.roger_last; const string _title = Create_Account.roger_job_title; const string _email = Create_Account.roger_email; const string _password = Create_Account.roger_password;

        [Test, Order(40)]
        public async Task NavigateToSubmissionPage()       
        {
            Submission_Helper.NavigateToSubmission(this, Submission.OrgName_Blackpool, "2020", "2021");

            ExpectHeader("Review 2019 to 2020 group report for " + Submission.OrgName_Blackpool);
            await Task.CompletedTask;
        }

        [Test, Order(42)]
        public async Task ExpectLinks()
        {
            //expect links
            //todo add try command once implemented
            ExpectLink("Your modern slavery statement");
            ExpectLink("Areas covered by your modern slavery statement");
            ExpectLink("Your organisation");
            ExpectLink("Policies");
            ExpectLink("Supply chain risks and due diligence (part 1)");
            ExpectLink("Supply chain risks and due diligence (part 2)");
            ExpectLink("Training");
            ExpectLink("Monitoring progress");
            await Task.CompletedTask;
        }

        [Test, Order(42)]
        public async Task TestLinkNavigation()
        {
            //following link to section on this page should return to this screen once completed
            SaveAndCancelcheck(this, "Your modern slavery statement", "Your modern slavery statement", Submission.OrgName_Blackpool);
            SaveAndCancelcheck(this, "Areas covered by your modern slavery statement", "Areas covered by your modern slavery statement", Submission.OrgName_Blackpool);
            SaveAndCancelcheck(this, "Your organisation", "Your organisation", Submission.OrgName_Blackpool);
            SaveAndCancelcheck(this, "Policies", "Policies", Submission.OrgName_Blackpool);
            SaveAndCancelcheck(this, "Supply chain risks and due diligence (part 1)", "Supply Chain Risks and due diligence", Submission.OrgName_Blackpool);
            SaveAndCancelcheck(this, "Supply chain risks and due diligence (part 2)", "Supply Chain Risks and due diligence", Submission.OrgName_Blackpool);
            SaveAndCancelcheck(this, "Training", "Training", Submission.OrgName_Blackpool);
            SaveAndCancelcheck(this, "Monitoring progress", "Monitoring progress", Submission.OrgName_Blackpool);
           
            await Task.CompletedTask;
        }

        private static void SaveAndCancelcheck(UIContext ui, string LinkText, string HeaderText, string OrgName) 
        { 
            //navigate to page
            ui.ExpectHeader("Review 2019 to 2020 group report for " + OrgName);
            ui.ClickLink(LinkText);
            ui.ExpectHeader(HeaderText);

            //test cancel/save cancel/cancel and continue all return to draft page
            ui.Click("Continue");
            ui.ExpectHeader("Review 2019 to 2020 group report for " + OrgName);
           
            ui.ClickLink(LinkText);
            ui.ExpectHeader(HeaderText);
            ui.Click("Cancel");
            ui.Expect("You have unsaved data");
            ui.Click("Exit without saving");
            ui.ExpectHeader("Review 2019 to 2020 group report for " + OrgName);

            ui.ClickLink(LinkText);
            ui.ExpectHeader(HeaderText);
            ui.Click("Cancel");
            ui.Expect("You have unsaved data");
            ui.Click("Save the data");
            ui.Expect(What.Contains, "You`ve saved a draft of your Modern Slavery Statement");
            ui.Click("Continue");
            ui.ExpectHeader("Review 2019 to 2020 group report for " + OrgName);
        }
    }
}