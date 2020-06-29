using Pangolin;
using Pangolin.Helper.UIContext;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class Review_And_Edit_Link_Navigation : UITest
    {
        [TestMethod]
        public override void RunTest()
        {
            //user registered to org
            LoginAs<RogerReporter>();

            Submission_Helper.NavigateToSubmission(this, Submission.OrgName_Blackpool, "2020", "2021");

            ExpectHeader("Review 2019 to 2020 group report for " + Submission.OrgName_Blackpool);


            //expect links
            ExpectLink("Your modern slavery statement");
            ExpectLink("Areas covered by your modern slavery statement");
            ExpectLink("Your organisation");
            ExpectLink("Policies");
            ExpectLink("Supply chain risks and due diligence (part 1)");
            ExpectLink("Supply chain risks and due diligence (part 2)");
            ExpectLink("Training");
            ExpectLink("Monitoring progress");

            //following link to section on this page should return to this screen once completed
            SaveAndCancelcheck(this, "Your modern slavery statement", "Your modern slavery statement", Submission.OrgName_Blackpool);
            SaveAndCancelcheck(this, "Areas covered by your modern slavery statement", "Areas covered by your modern slavery statement", Submission.OrgName_Blackpool);
            SaveAndCancelcheck(this, "Your organisation", "Your organisation", Submission.OrgName_Blackpool);
            SaveAndCancelcheck(this, "Policies", "Policies", Submission.OrgName_Blackpool);
            SaveAndCancelcheck(this, "Supply chain risks and due diligence (part 1)", "Supply Chain Risks and due diligence", Submission.OrgName_Blackpool);
            SaveAndCancelcheck(this, "Supply chain risks and due diligence (part 2)", "Supply Chain Risks and due diligence", Submission.OrgName_Blackpool);
            SaveAndCancelcheck(this, "Training", "Training", Submission.OrgName_Blackpool);
            SaveAndCancelcheck(this, "Monitoring progress", "Monitoring progress", Submission.OrgName_Blackpool);
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