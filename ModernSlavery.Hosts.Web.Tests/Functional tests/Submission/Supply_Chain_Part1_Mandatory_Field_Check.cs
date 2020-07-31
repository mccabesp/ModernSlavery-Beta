using Geeks.Pangolin;
using Geeks.Pangolin.Helper.UIContext;
using NUnit.Framework;
using System.Threading.Tasks;

namespace ModernSlavery.Hosts.Web.Tests
{
    [TestFixture, Ignore("Awaiting Submission merge")]

    public class Supply_Chain_Part1_Mandatory_Field_Check : Private_Registration_Success
    {
        [Test, Order(40)]
        public async Task NavigateToSCRPart1()
        {
            Submission_Helper.NavigateToSupplyChainRisks1(this, Submission.OrgName_Blackpool, "2019/2020");
            ExpectHeader("Supply Chain Risks and due diligence");
            await Task.CompletedTask;
        }

        [Test, Order(42)]
        public async Task TriggerAllOtherFieldsToAppear()
        {
            //only mandatory fields are other fields

            //trigger all other fields by clicking all "other" options
            string[] OtherOptions = new string[] { "Other vulnerable group", "Other type of work", "Please specify the other sector" };

            foreach (var Option in OtherOptions)
            ClickLabel(Option);

            await Task.CompletedTask;
        }

        [Test, Order(44)]
        public async Task SubmitFormWithoutFillingInOptionDetails()
        {
            Click("Continue");
            await Task.CompletedTask;
        }

        [Test, Order(46)]
        public async Task CheckValidationMessages()
        {
            Expect("There is a problem");
            Expect("Please enter explanation");
            await Task.CompletedTask;
        }

        [Test, Order(48)]
        public async Task FillInMandatoryFields()
        {
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
            CopyUrl("Part 1");

            await Task.CompletedTask;
        }
        [Test, Order(49)]
        public async Task ClickingCOntinueGoesToPart2()
        {
            //expect new page

            Click("Continue");
            Expect("Part 2");
            await Task.CompletedTask;
        }

        [Test, Order(50)]
        public async Task ReturnToPart1()
        {
            GotoCopiedUrl("Part 1");

            Expect("Part 1");
            await Task.CompletedTask;
        }

        [Test, Order(52)]
        public async Task PleaseExplainWhyChecks()
        {
            //when selecting option additional mandatory field appears
            PleaseExplainWhyCheck(this, Submission.GoodsAndServices, "Part 1");
            PleaseExplainWhyCheck(this, Submission.VulnerableGroups, "Part 1");
            PleaseExplainWhyCheck(this, Submission.TypesOfWork, "Part 1");
            PleaseExplainWhyCheck(this, Submission.Sectors, "Part 1");
            PleaseExplainWhyCheck(this, Submission.GoodsAndServices, "Part 1");
            await Task.CompletedTask;
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