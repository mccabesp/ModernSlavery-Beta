﻿using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class Supply_Chain_Risks_Part1_Q1_Interaction_Check : UITest
    {
        [TestMethod]
        public override void RunTest()
        {
            LoginAs<RogerReporter>();

            Submission_Helper.NavigateToSupplyChainRisks1(this, Submission.OrgName_Blackpool, "2019/2020");
            ExpectHeader("Supply Chain Risks and due diligence");

            //goods and services
            Submission_Helper.ExpandSection(this, "Goods and Services", "1");
            foreach (var GoodOrService in Submission.GoodsAndServices)
            {
                NearHeader("Goods and Services").Click(GoodOrService);
            }
            NearHeader("Goods and Services").Expect(Submission.SupplyChainRisks_SelectedGoodsAndServices.Length + " Selected");
            Submission_Helper.ColapseSection(this, "Goods and Services", "1");

            //Vulnerable Groups
            Submission_Helper.ExpandSection(this, "Vulnerable groups", "1");

            foreach (var VulnerableGroup in Submission.VulnerableGroups)
            {
                NearHeader("Vulnerable groups").ClickLabel(VulnerableGroup);
                if (VulnerableGroup == "Other vulnerable groups")                {
                    Set("Please specify the vulnerable group and explain why").To(Submission.SupplyChainRisks_OtherVulernableGroupsDetails);
                
            }
            NearHeader("Vulnerable groups").Expect(Submission.SupplyChainRisks_SelectedVulnerableGroups.Length + " Selected");

            Submission_Helper.ColapseSection(this, "Goods and Services", "1");


            //Type of work
            Submission_Helper.ExpandSection(this, "Type of work", "1");

            foreach (var TypeOfWork in Submission.TypesOfWork)
            {
                NearHeader("Type of work").ClickLabel(TypeOfWork);
                if (TypeOfWork == "Other type of work")                {
                    Set("Please specify the other type of work and explain why").To(Submission.SupplyChainRisks_OtherTypeOfWorkDetails);
                
            }
            NearHeader("Type of work").Expect(Submission.SupplyChainRisks_SelectedTypeOfWorks.Length + " Selected");

            Submission_Helper.ColapseSection(this, "Type of work", "1");

            //Sectors
            Submission_Helper.ExpandSection(this, "Sectors", "1");

            foreach (var Sector in Submission.SupplyChainSectors)
            {
                NearHeader("Sectors").ClickLabel(Sector);

                if (Sector == "Other sector")                
                    Set("Please specify the other sector and explain why").To(Submission.SupplyChainRisks_OtherSectorDetails);
              
            }

            Set("If you want to specify an area not mentioned above, please provide details").To(Submission.SuppliChainRisks_OtherArea);


            NearHeader("Sectors").Expect(Submission.SupplyChainRisks_SelectedSectors.Length + " Selected");



            Submission_Helper.ColapseSection(this, "Type of work", "1");
        }
    }
}