using Geeks.Pangolin;
using NUnit.Framework;
using System.Threading.Tasks;

namespace ModernSlavery.Hosts.Web.Tests
{
    [TestFixture, Ignore("Awaiting Submission merge")]

    public class Submission_Complete_All_Sections : Private_Registration_Success
    {
        [Test, Order(40)]
        public async Task StartSubmission()
        {

            ExpectHeader("Select an organisation");

            Click(Submission.OrgName_Blackpool);

            ExpectHeader("Manage your organisations reporting");

            AtRow("2019/20").Click("Draft report");

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

            AtLabel("Your organisation’s structure, business and supply chains").ClickLabel("Yes");
            AtLabel("Policiess").ClickLabel("Yes");
            AtLabel("Risk assessment and management").ClickLabel("Yes");
            AtLabel("Due diligence processes").ClickLabel("Yes");
            AtLabel("Staff training about slavery and human trafficking").ClickLabel("Yes");
            AtLabel("Goals and key performance indicators (KPIs) to measure your progress over time, and the effectiveness of your actions").ClickLabel("Yes");

            Click("Continue");
            await Task.CompletedTask;
        }

        [Test, Order(46)]
        public async Task YourOrganisation()
        { 
            ExpectHeader("Your organisation");

            foreach (var sector in Submission.YourOrganisation_Sectors)
            {
                ClickLabel(sector);
            }

            Set("What was your turnover or budget during the last financial accounting year?").To(Submission.YourOrganisation_Turnover);
            Click("Continue");

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
                    Set("Please provide detail").To(Submission.Policies_OtherDetails);
                }
            }

            Click("Continue");
            await Task.CompletedTask;
        }

        [Test, Order(50)]
        public async Task SCRPart1()
        {

            ExpectHeader("Supply Chain Risks and due diligence");

            //goods and services
            Submission_Helper.ExpandSection(this, "Goods and Services", "1");
            foreach (var GoodOrService in Submission.SupplyChainRisks_SelectedGoodsAndServices)
            {
                NearHeader("Goods and Services").Click(GoodOrService);               
            }
            NearHeader("Goods and Services").Expect(Submission.SupplyChainRisks_SelectedGoodsAndServices.Length + " Selected");
            Submission_Helper.ColapseSection(this, "Goods and Services", "1");

            //Vulnerable Groups
            Submission_Helper.ExpandSection(this, "Vulnerable groups", "1");
            
            foreach (var VulnerableGroup in Submission.SupplyChainRisks_SelectedVulnerableGroups)
            {
                NearHeader("Vulnerable groups").ClickLabel(VulnerableGroup);
                if (VulnerableGroup == "Other vulnerable groups")
                {
                    Set("Please specify the vulnerable group and explain why").To(Submission.SupplyChainRisks_OtherVulernableGroupsDetails);
                }
            }
            NearHeader("Vulnerable groups").Expect(Submission.SupplyChainRisks_SelectedVulnerableGroups.Length + " Selected");
            
            Submission_Helper.ColapseSection(this, "Goods and Services", "1");


            //Type of work
            Submission_Helper.ExpandSection(this, "Type of work", "1");

            foreach (var TypeOfWork in Submission.SupplyChainRisks_SelectedTypeOfWorks)
            {
                NearHeader("Type of work").ClickLabel(TypeOfWork);
                if (TypeOfWork == "Other type of work")
                {
                    Set("Please specify the other type of work and explain why").To(Submission.SupplyChainRisks_OtherTypeOfWorkDetails);
                }
            }
            NearHeader("Type of work").Expect(Submission.SupplyChainRisks_SelectedTypeOfWorks.Length + " Selected");

            Submission_Helper.ColapseSection(this, "Type of work", "1");

            //Sectors
            Submission_Helper.ExpandSection(this, "Sectors", "1");

            foreach (var Sector in Submission.SupplyChainRisks_SelectedSectors)
            {
                NearHeader("Sectors").ClickLabel(Sector);
               
                    if (Sector == "Other sector")
                    {
                    Set("Please specify the other sector and explain why").To(Submission.SupplyChainRisks_OtherSectorDetails);
                    }            
            }

            Set("If you want to specify an area not mentioned above, please provide details").To(Submission.SuppliChainRisks_OtherArea);


            NearHeader("Sectors").Expect(Submission.SupplyChainRisks_SelectedSectors.Length + " Selected");
            
            
            
            Submission_Helper.ColapseSection(this, "Type of work", "1");


            //countries
            Submission_Helper.CountrySelect(this, "Africa", Submission.SupplyChainRisks_SelectedCountriesAfrica);
            Submission_Helper.CountrySelect(this, "Asia", Submission.SupplyChainRisks_SelectedCountriesAsia);
            Submission_Helper.CountrySelect(this, "Europe", Submission.SupplyChainRisks_SelectedCountriesEurope);
            Submission_Helper.CountrySelect(this, "North America", Submission.SupplyChainRisks_SelectedCountriesNorthAmerica);
            Submission_Helper.CountrySelect(this, "Oceania", Submission.SupplyChainRisks_SelectedCountriesOceania);
            Submission_Helper.CountrySelect(this, "South America", Submission.SupplyChainRisks_SelectedCountriesSouthAmerica);
            Submission_Helper.CountrySelect(this, "Antarctica", Submission.SupplyChainRisks_SelectedCountriesAntarctica);
            
            Click("Continue");
            ExpectHeader("Supply Chain and Due diligence");


            Click("Continue");
            await Task.CompletedTask;
        }

        [Test, Order(52)]
        public async Task SCRPart2()
        {
            //Partnerships   
            Submission_Helper.ChekcboxSelector(this, "Partnerships", Submission.SupplyChainRisks_SelectedPartnerships);

            //social audits
            Submission_Helper.ChekcboxSelector(this, "Social audits", Submission.SupplyChainRisks_SelectedSocialAudits, 
                OtherOption: "other",
                OtherFieldLabel: "Please specify the other social audits",
                OtherDetails: Submission.SupplyChainRisks_OtherSocialAudits);

            //Anonymous grievance mechanism
            Submission_Helper.ChekcboxSelector(this, "Anonymous grievance mechanism", Submission.SupplyChainRisks_SelectedGrievanceMechanisms);

            //indicators
            BelowHeader("For the period of this statement, have you identified any potential indicators of forced labour or modern slavery in your operations or supply chain?").Set("Examples include no formal identification, or who are always dropped off and collected in the same way, often late at night or early in the morning.").To("Yes");
            BelowHeader("For the period of this statement, have you identified any potential indicators of forced labour or modern slavery in your operations or supply chain?").Set(The.Top, "Provide detail").To(Submission.SupplyChainRisks_IndicatorDetails);


            //instances
            BelowHeader("Have you or anyone else found instances of modern slavery in your operations or supply chain in the last year?").Click(The.Top, "Yes");
            BelowHeader("Have you or anyone else found instances of modern slavery in your operations or supply chain in the last year?").Set(The.Top, "Provide detail").To(Submission.SupplyChainRisks_InStanceDetails);

            //Remidiation Actions
            Submission_Helper.ChekcboxSelector(this, "What remediation action did your organisation take in response?", Submission.SupplyChainRisks_SelectedRemediationActions);

            Click("Continue");
            ExpectHeader("Training");

            await Task.CompletedTask;
        }

        [Test, Order(54)]
        public async Task Training()
        {
            //Training
            Submission_Helper.ChekcboxSelector(this, "Training", Submission.SelectedTrainings,
                OtherOption: "other",
                OtherFieldLabel: "Please specify",
                OtherDetails: Submission.OtherTrainings,
                NeedExpand: false);

            Click("Continue");
            ExpectHeader("Monitoring progress");
            await Task.CompletedTask;
        }

        [Test, Order(56)]
        public async Task MonitoringProgress()
        {
            BelowHeader("Does you modern slavery statement include goals relating to how you will prevent modern slavery in your operations and supply chains?").ClickLabel("Yes");

            Set("How is your organisation measuring progress towards these goals?").To(Submission.MonitoringProgress);
            Set("What were your key achievements in relation to reducing modern slavery during the period covered by this statement?").To(Submission.MonitoringAchievements);

            Set("If your statement is for a group of organisations, please select the answer that applies to the organisation with the longest history of producing statements.").To("1 - 5 years");

            Click("Continue");
            ExpectHeader("Review 2019 to 2020 group report for" + Submission.OrgName_Blackpool);

            //all sections should be completed
            AtRow("Your modern Slavery statement").Expect("Completed");
            AtRow("Areas covered by your modern slavery statement").Expect("Completed");

            //all other sections incomplete 

            AtRow("Your organisation").Expect("Completed");
            AtRow("Policies").Expect("Completed");
            AtRow("Supply chain risks and due diligence (part 1)").Expect("Completed");
            AtRow("Supply chain risks and due diligence (part 2)").Expect("Completed");
            AtRow("Training").Expect("Completed");
            AtRow("Monitoring progress").Expect("Completed");

            ExpectButton("Confirm and submit");
            ExpectButton("Save draft");

            Click("Save draft");
            ExpectHeader("Select an organisation");

            await Task.CompletedTask;
        }
    }
    }
