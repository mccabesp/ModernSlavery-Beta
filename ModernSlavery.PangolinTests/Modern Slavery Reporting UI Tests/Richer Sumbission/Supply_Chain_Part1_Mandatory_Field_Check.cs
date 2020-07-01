using Pangolin;
using Pangolin.Helper.UIContext;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class Supply_Chain_Part1_Mandatory_Field_Check : UITest
    {
        [TestMethod]
        public override void RunTest()
        {
            LoginAs<RogerReporter>();

            Submission_Helper.NavigateToSupplyChainRisks1(this, Submission.OrgName_Blackpool, "2019/2020");
            ExpectHeader("Supply Chain Risks and due diligence");

            //only mandatory fields are other fields

            //trigger all other fields by clicking all "other" options
            string[] OtherOptions = new string[] { "Other vulnerable group", "Other type of work", "Please specify the other sector" };

            foreach (var Option in OtherOptions)
            ClickLabel(Option);


            Click("Continue");

            Expect("There is a problem");
            Expect("Please enter explanation");

            //all fields must be filled
            var OtherFields = new string[] { "Please specify the other vulnerable group", "Please specify the other type of work", "Please specify the other sector" };
            foreach (var  Field in OtherFields)
            {
                Set("Please specify the other vulnerable group").To("Details");
                Click("Continue");

                Expect("There is a problem");
                Expect("Please enter explanation");
            }

            //set last field
            Set("If you want to specify an area not mentioned above, please provide details").To("Details");
            Click("Continue");

            CopyUrl("Part 1");
            //expect new page
            Expect("Part 2");

            GotoCopiedUrl("Part 1");

            Expect("Part 1");

            //when selecting option additional mandatory field appears
            PleaseExplainWhyCheck(this, Submission.GoodsAndServices, "Part 1");
            PleaseExplainWhyCheck(this, Submission.VulnerableGroups, "Part 1");
            PleaseExplainWhyCheck(this, Submission.TypesOfWork, "Part 1");
            PleaseExplainWhyCheck(this, Submission.Sectors, "Part 1");
            PleaseExplainWhyCheck(this, Submission.GoodsAndServices, "Part 1");
        }

        private static void PleaseExplainWhyCheck(UIContext ui, string[] options, string URL) 
        {
            foreach (var Option  in options)
            {
                ui.BelowLabel(Option).ExpectNoField("Option");
                ui.ClickLabel(Option);
                ui.BelowLabel(Option).ExpectField("Please explain why");
                ui.Click("Continue");
                ui.Expect("There is a problem");

                ui.BelowLabel(Option).Set(The.Top, "Please explain why").To("Reason");
                ui.Click("Continue");
                ui.Expect("Part 2");
                ui.GotoCopiedUrl(URL);
            }
        }

    }
}