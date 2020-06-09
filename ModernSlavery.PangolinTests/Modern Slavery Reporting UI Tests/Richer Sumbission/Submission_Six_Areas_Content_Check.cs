using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class Submission_Six_Areas_Content_Check : UITest
    {
        [TestMethod]
        public override void RunTest()
        {
            Run<Fastrack_Registraion_Success>();

            LoginAs<RogerReporter>();

            ExpectHeader("Select an organisation");

            Click(Submission.OrgName_Blackpool);

            ExpectHeader("Manage your organisations reporting");

            Click("Draft Report");

            ExpectHeader("Your modern slavery statement");
            Set(The.Top, "Date").To(Submission.DateFrom_Blackpool);
            Set("First name").To(Submission.FirstName_Blackpool);
            Set("Last name").To(Submission.LastName_Blackpool);
            Set("Job title").To(Submission.JobTitle_Blackpool);
            Set("Url").To(Submission.Url_Blackpool);
            Set(The.Bottom, "Date").To(Submission.DateApproved_Blackpool);

            Click("Save and continue");

            ExpectHeader("Six areas of modern slavery statement");
            ExpectText("Does your statement include details of the steps your organisation has taken in the following six areas?");
            ExpectLink("What are the 6 areas");
            AtLabel("1. Your organisation’s structure, its business and its supply chains?").ExpectLabel("Yes");
            AtLabel("1. Your organisation’s structure, its business and its supply chains?").ExpectLabel("No");
            AtLabel("2. Your policies in relation to slavery and human trafficking?").ExpectLabel("Yes");
            AtLabel("2. Your policies in relation to slavery and human trafficking?").ExpectLabel("No");
            AtLabel("3. Due diligence processes in relation to slavery and human trafficking in your business and supply chains?").ExpectLabel("Yes");
            AtLabel("3. Due diligence processes in relation to slavery and human trafficking in your business and supply chains?").ExpectLabel("No");
            AtLabel("4. The areas of your business and supply chains where there is a risk of slavery and human trafficking taking place, and the steps you have taken to assess and manage that risk?").ExpectLabel("Yes");
            AtLabel("4. The areas of your business and supply chains where there is a risk of slavery and human trafficking taking place, and the steps you have taken to assess and manage that risk?").ExpectLabel("No");
            AtLabel("5. How effective your organisation has been in ensuring that slavery and human trafficking is not taking place in your business or supply chains, and what performance indicators you are using to measure your effectiveness?").ExpectLabel("Yes");
            AtLabel("5. How effective your organisation has been in ensuring that slavery and human trafficking is not taking place in your business or supply chains, and what performance indicators you are using to measure your effectiveness?").ExpectLabel("No"); 
            AtLabel("6. Staff training about slavery and human trafficking?").ExpectLabel("Yes");
            AtLabel("6. Staff training about slavery and human trafficking?").ExpectLabel("No");

            ExpectButton("Save and continue");
            ExpectButton("Cancel");
        }
    }
}