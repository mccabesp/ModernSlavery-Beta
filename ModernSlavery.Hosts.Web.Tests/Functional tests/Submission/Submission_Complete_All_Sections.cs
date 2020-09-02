using Geeks.Pangolin;
using NUnit.Framework;
using System.Threading.Tasks;

namespace ModernSlavery.Hosts.Web.Tests
{
    [TestFixture, Ignore("Temporary Ignore")]
    public class Submission_Complete_All_Sections : Private_Registration_Success
    {
        [Test, Order(40)]
        public async Task StartSubmission()
        {

            ExpectHeader("Select an organisation");

            Click(Submission.OrgName_InterFloor);

            ExpectHeader(That.Contains, "Manage your modern slavery statement submissions");

            
            Click("Start Draft");

            ExpectHeader("Before you start");
            Click("Start Now");

            await Task.CompletedTask;
        }

        [Test, Order(42)]
        public async Task YourModernSlaveryStatement()
        {
            
        ExpectHeader("Your modern slavery statement");

            Set("URL").To(Submission.YourMSStatement_URL);

            Submission_Helper.DateSet(this, Submission.YourMSStatement_To_Day, Submission.YourMSStatement_To_Month, Submission.YourMSStatement_To_Year, "1");
            Submission_Helper.DateSet(this, Submission.YourMSStatement_To_Day, Submission.YourMSStatement_To_Month, Submission.YourMSStatement_To_Year, "2");

            Set("First name").To(Submission.YourMSStatement_First);
            Set("Last name").To(Submission.YourMSStatement_Last);
            Set("Job title").To(Submission.YourMSStatement_JobTitle);

            Submission_Helper.DateSet(this, Submission.YourMSStatement_ApprovalDate_Day, Submission.YourMSStatement_ApprovalDate_Month, Submission.YourMSStatement_ApprovalDate_Year, "3");

            Click("Continue");
            await Task.CompletedTask;
        }

        [Test, Order(44)]
        public async Task AreasCoveredByYourModernSlaveryStatement()
        {
            ExpectHeader("Areas covered by your modern slavery statement");

            BelowHeader("Your organisation’s structure, business and supply chains").ClickLabel(The.Top, "Yes");
            BelowHeader("Policies").ClickLabel(The.Top, "Yes");
            BelowHeader("Risk assessment and management").ClickLabel(The.Top, "Yes");
            BelowHeader("Due diligence processes").ClickLabel(The.Top, "Yes");
            BelowHeader("Staff training about slavery and human trafficking").ClickLabel(The.Top, "Yes");
            BelowHeader("Goals and key performance indicators (KPIs) to measure your progress over time, and the effectiveness of your actions").ClickLabel(The.Top, "Yes");

            Click("Continue");
            await Task.CompletedTask;
        }

        [Test, Order(46)]
        public async Task YourOrganisation()
        { 
            ExpectHeader(That.Contains, "Your organisation");

            foreach (var sector in Submission.YourOrganisation_Sectors)
            {
                ClickLabel(sector);
            }

            //Set("What was your turnover or budget during the last financial accounting year?").To(Submission.YourOrganisation_Turnover);
            ExpectLabel("Please specify");
            ClickLabel(Submission.YourOrganisation_Turnover);
            Set("OtherSector").To("Other details");
            Click("Continue");

            await Task.CompletedTask;
        }

        [Test, Order(48)]
        public async Task Policies()
        {
            ExpectHeader("Policies");

            foreach (var Policy in Submission.Policies_SelectedPolicies)
            {
                ClickLabel(Policy);

                //fill in other details
                if (Policy == "Other")
                {
                    Set("OtherPolicies").To(Submission.Policies_OtherDetails);
                    ExpectLabel("Please provide details");
                }
            }

            Click("Continue");
            await Task.CompletedTask;
        }

        [Test, Order(50)]
        public async Task SCRPart1()
        {

            ExpectHeader(That.Contains, "Supply chain risks and due diligence");

            //goods and services
            Submission_Helper.ExpandSection(this, "Goods and Services", "1");
            foreach (var GoodOrService in Submission.SupplyChainRisks_SelectedGoodsAndServices)
            {
                NearHeader(The.Top, "Goods and Services").Click(GoodOrService);               
            }
            //NearHeader(The.Top, "Goods and Services").Expect(Submission.SupplyChainRisks_SelectedGoodsAndServices.Length + " Selected");
            Submission_Helper.ColapseSection(this, "Goods and Services", "1");

            //Vulnerable Groups
            Submission_Helper.ExpandSection(this, "Vulnerable groups", "1");
            
            foreach (var VulnerableGroup in Submission.SupplyChainRisks_SelectedVulnerableGroups)
            {
                NearHeader(The.Top, "Vulnerable groups").ClickLabel(VulnerableGroup);
                if (VulnerableGroup == "Other vulnerable group(s)")
                {
                    Set("Please specify").To(Submission.SupplyChainRisks_OtherVulernableGroupsDetails);
                }
            }
           // NearHeader(The.Top, "Vulnerable groups").Expect(Submission.SupplyChainRisks_SelectedVulnerableGroups.Length + " Selected");
            
            Submission_Helper.ColapseSection(this, "Goods and Services", "1");


            //Type of work
            Submission_Helper.ExpandSection(this, "Type of work", "1");

            foreach (var TypeOfWork in Submission.SupplyChainRisks_SelectedTypeOfWorks)
            {
                NearHeader(The.Top, "Type of work").ClickLabel(TypeOfWork);
                if (TypeOfWork == "Other type of work")
                {
                    BelowHeader(The.Top, "Type of work").Set(The.Top, "Please specify").To(Submission.SupplyChainRisks_OtherTypeOfWorkDetails);
                }
            }
            //NearHeader(The.Top, "Type of work").Expect(Submission.SupplyChainRisks_SelectedTypeOfWorks.Length + " Selected");

            Submission_Helper.ColapseSection(this, "Type of work", "1");

            //Sectors
            Submission_Helper.ExpandSection(this, "Sectors", "1");

            foreach (var Sector in Submission.SupplyChainRisks_SelectedSectors)
            {
                NearHeader(The.Top, "Sectors").ClickLabel(Sector);
               
                    if (Sector == "Other sector")
                    {
                    BelowHeader(The.Top, "Sectors").Set(The.Top, "Please specify").To(Submission.SupplyChainRisks_OtherSectorDetails);
                    }            
            }

            Set("If you want to specify an area not mentioned above, please provide details").To(Submission.SuppliChainRisks_OtherArea);


            //NearHeader(The.Top, "Sectors").Expect(What.Contains, Submission.SupplyChainRisks_SelectedSectors.Length.ToString() + " Selected");
            
            
            
            Submission_Helper.ColapseSection(this, "Type of work", "1");

            //goods and services - highest risk
            Submission_Helper.ExpandSection(this, "Goods and Services", "2");
            foreach (var GoodOrService in Submission.SupplyChainRisks_SelectedGoodsAndServices)
            {
                NearHeader(The.Bottom, "Goods and Services").Click(GoodOrService);
                NearHeader(The.Bottom, "Goods and Services").Below(GoodOrService).Set(The.Top).To(GoodOrService + "Details");
            }
            Submission_Helper.ColapseSection(this, "Goods and Services", "2");

            

            Set("If you want to specify an area not mentioned above, please provide details").To(Submission.SuppliChainRisks_OtherArea);


            //countries
            Submission_Helper.CountrySelect(this, "Africa", Submission.SupplyChainRisks_SelectedCountriesAfrica);
            Submission_Helper.CountrySelect(this, "Asia", Submission.SupplyChainRisks_SelectedCountriesAsia);
            Submission_Helper.CountrySelect(this, "Europe", Submission.SupplyChainRisks_SelectedCountriesEurope);
            Submission_Helper.CountrySelect(this, "North America", Submission.SupplyChainRisks_SelectedCountriesNorthAmerica);
            Submission_Helper.CountrySelect(this, "Oceania", Submission.SupplyChainRisks_SelectedCountriesOceania);
            Submission_Helper.CountrySelect(this, "South America", Submission.SupplyChainRisks_SelectedCountriesSouthAmerica);
            Submission_Helper.CountrySelect(this, "Antarctica", Submission.SupplyChainRisks_SelectedCountriesAntarctica);
            
            Click("Continue");
            ExpectHeader(That.Contains, "Supply chain risks and due diligence");


            await Task.CompletedTask;
        }

        [Test, Order(52)]
        public async Task SCRPart2()
        {
            //Partnerships   
            Submission_Helper.ChekcboxSelector(this, "Partnerships", Submission.SupplyChainRisks_SelectedPartnerships);

            //social audits
            Submission_Helper.ChekcboxSelector(this, "Social audits", Submission.SupplyChainRisks_SelectedSocialAudits, 
                OtherOption: "other type of social audit",
                OtherFieldLabel: "Please specify",
                OtherDetails: Submission.SupplyChainRisks_OtherSocialAudits);

            //Anonymous grievance mechanism
            Submission_Helper.ChekcboxSelector(this, "Anonymous grievance mechanisms", Submission.SupplyChainRisks_SelectedGrievanceMechanisms);

            //indicators
            BelowHeader("For the period of this statement, have you identified any potential indicators of forced labour or modern slavery in your operations or supply chain?").Expect("Examples include workers with no formal identification, or who are always dropped off and collected in the same way, often late at night or early in the morning.");

            BelowHeader("For the period of this statement, have you identified any potential indicators of forced labour or modern slavery in your operations or supply chain?").ClickLabel(The.Top, "Yes");

            BelowHeader("For the period of this statement, have you identified any potential indicators of forced labour or modern slavery in your operations or supply chain?").Set(The.Top, "Please provide details").To(Submission.SupplyChainRisks_IndicatorDetails);

            //instances
            BelowHeader("Have you or anyone else found instances of modern slavery in your operations or supply chain in the last year?").ClickLabel(The.Top, "Yes");
            BelowHeader("Have you or anyone else found instances of modern slavery in your operations or supply chain in the last year?").Set(The.Top, "Please provide details").To(Submission.SupplyChainRisks_InStanceDetails);

            //Remidiation Actions
            BelowHeader("Did your organisation take any remediation actions in response?").ClickLabel(The.Top, "Yes");

            //label[contains(text(), 'repayment of recruitment fees')]/preceding-sibling::input
            //sSubmission_Helper.ChekcboxSelector(this, "What actions did your organisation take?", Submission.SupplyChainRisks_SelectedRemediationActions, NeedExpand: false);
            //ClickXPath("/html/body/div/main/div/div/form/fieldset[3]/div/div/div[2]/div[2]/div[2]/div/fieldset/div/div[1]/input");
            //foreach (var Action in Submission.SupplyChainRisks_SelectedRemediationActions)
            //{
            //    ClickXPath("//label[contains(text(), '" + Action +"')]/preceding-sibling::input");

            //    if (Action == "other")
            //    {
            //        Set(The.Bottom, "Please specify").To("Other details");
            //    }
            //}

            ClickHeader("What actions did your organisation take?");
            Press(Keys.Tab);
            Press(Keys.Space);
            for (int i = 0; i < 5; i++)
            {
                Press(Keys.Tab);
            }
            Press(Keys.Space);

            Set("OtherRemediation").To("Other details");


            Click("Continue");
            ExpectHeader("Training");

            await Task.CompletedTask;
        }

        [Test, Order(54)]
        public async Task Training()
        {
            //Training
            Submission_Helper.ChekcboxSelector(this, "Training", Submission.SelectedTrainings,
                OtherOption: "Other",
                OtherFieldLabel: "OtherTraining",
                OtherDetails: Submission.OtherTrainings,
                NeedExpand: false);

            Click("Continue");
            ExpectHeader("Monitoring progress");
            await Task.CompletedTask;
        }

        [Test, Order(56)]
        public async Task MonitoringProgress()
        {
            BelowHeader("Does your modern slavery statement include goals relating to how you will prevent modern slavery in your operations and supply chains?").ClickLabel("No");

            Set("How is your organisation measuring progress towards these goals?").To(Submission.MonitoringProgress);
            BelowLabel("What were your key achievements in relation to reducing modern slavery during the period covered by this statement?").Set(The.Top).To(Submission.MonitoringAchievements);

            BelowHeader("How many years has your organisation been producing modern slavery statements?").Below("If your statement is for a group of organisations, please select the answer that applies to the organisation with the longest history of producing statements.").ClickLabel("1 to 5 years");

            Click("Continue");
            ExpectHeader("Review before submitting");

            //all sections should be completed
            RightOf("Your modern Slavery statement").Expect("Completed");
            RightOf("Areas covered by your modern statement").Expect("Completed");


            RightOf("Your organisation").Expect("Completed");
            RightOf("Policies").Expect("Completed");
            RightOf("Supply chain risks and due diligence (part 1)").Expect("Completed");
            RightOf("Supply chain risks and due diligence (part 2)").Expect("Comzpleted");
            RightOf("Training").Expect("Completed");
            RightOf("Monitoring progress").Expect("Completed");

            ExpectButton("Confirm and submit");
            ExpectButton("Exit and save Changes");
            ExpectButton("Exit and lose Changes");

            Click("Exit and save Changes");
            ExpectHeader(That.Contains, "Manage your modern slavery statement submissions");            

            await Task.CompletedTask;
        }
    }
    }
