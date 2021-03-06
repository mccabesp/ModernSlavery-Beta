﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Geeks.Pangolin;
using Geeks.Pangolin.Helper.UIContext;
namespace ModernSlavery.Hosts.Web.Tests


{
    class Submission_Helper
    {
        public static void NavigateTo_SubmissionStarted(UIContext ui, string Organistion, string YearFromTo, string SectionText)
        {
            ui.Goto("/manage-organisations");

            //navigate to org page
            ui.Click("Your organisations");
            ui.ExpectHeader("Register or select organisations you want to add statements for");

            //select org
            ui.Click(Organistion);

            //select year's report
            ui.RightOfText(YearFromTo).Click(The.Top, "Continue");
            ui.ExpectHeader("Review before submitting");

            //select section
            ui.Click(SectionText);
        }

        public static void NavigateYourMSStatement(UIContext ui, string Organistion, string Year, bool MoreInfoRequired = false)
        {
            ui.Goto("/manage-organisations");
            ui.ExpectHeader("Register or select organisations you want to add statements for");

            ui.Click(Organistion);

            if (MoreInfoRequired)
            {
                ModernSlavery.Testing.Helpers.Extensions.SubmissionHelper.MoreInformationRequiredComplete(ui, MoreInfoRequired, OrgName: Organistion);
            }
            ui.ExpectHeader(That.Contains, "Manage your modern slavery statements");

            ui.RightOf(Year).Click(The.Top, "Start draft");

            ui.ExpectHeader("Before you start");
            ui.Click("Start now");
                    ModernSlavery.Testing.Helpers.Extensions.SubmissionHelper.GroupOrSingleScreenComplete(ui, OrgName: Organistion);
            ui.ExpectHeader("Your modern slavery statement");
        }
        public static void NavigateToAreasCovered(UIContext ui, string Organistion, string Year,  bool MoreInfoRequired = false)
        {
            NavigateYourMSStatement(ui, Organistion, Year, MoreInfoRequired: MoreInfoRequired);
            ui.Click("Continue");

            ui.ExpectHeader("Areas covered by your modern slavery statement");
        }
        public static void NavigateToYourOrganisation(UIContext ui, string Organistion, string Year, bool MoreInfoRequired = false)
        {
            NavigateToAreasCovered(ui, Organistion, Year, MoreInfoRequired: MoreInfoRequired );
            ui.Click("Continue");

            ui.ExpectHeader("Your organisation");
        }
        public static void NavigateToPolicies(UIContext ui, string Organistion, string Year, bool MoreInfoRequired = false)
        {
            NavigateToYourOrganisation(ui, Organistion, Year, MoreInfoRequired: MoreInfoRequired);
            ui.Click("Continue");
            ui.ExpectHeader("Policies");
        }
        public static void NavigateToSupplyChainRisks1(UIContext ui, string Organistion, string Year, bool MoreInfoRequired = false)
        {
            NavigateToPolicies(ui, Organistion, Year, MoreInfoRequired: MoreInfoRequired);
            ui.Click("Continue");

            ui.ExpectHeader(That.Contains, "Supply chain risks and due diligence");
            //ensure we are on the correct part of this section
            //there are 2
            ui.Expect("Part 1");
        }
        public static void NavigateToSupplyChainRisks2(UIContext ui, string Organistion, string Year, bool MoreInfoRequired = false)
        {
            NavigateToSupplyChainRisks1(ui, Organistion, Year, MoreInfoRequired: MoreInfoRequired);
            ui.Click("Continue");

            ui.ExpectHeader(That.Contains, "Supply chain risks and due diligence");
            
            //ensure we are on the correct part of this section
            //there are 2
            ui.Expect("Part 2");
        }
        public static void NavigateToTraining(UIContext ui, string Organistion, string Year, bool MoreInfoRequired = false)
        {
            NavigateToSupplyChainRisks2(ui, Organistion, Year, MoreInfoRequired: MoreInfoRequired);
            ui.Click("Continue");

            ui.ExpectHeader("Training");
        }
        public static void NavigateToMonitoringProgress(UIContext ui, string Organistion, string Year, bool MoreInfoRequired = false)
        {
            NavigateToTraining(ui, Organistion, Year, MoreInfoRequired: MoreInfoRequired);
            ui.Click("Continue");

            ui.ExpectHeader("Monitoring progress");
        }

        public static void NavigateToSectors(UIContext ui, string Organistion, string Year, bool MoreInfoRequired = false) {
            ui.Click("Mange Organisations");
            ui.ExpectHeader("Register or select organisations you want to add statements for");

            ui.Click("Organisation");
            ui.AtRow(Year).Click("Draft report");
            ui.ExpectHeader("Your modern slavery statement");

            ui.Click("Save and continue");
            ui.ExpectHeader("Six areas of modern slavery statement");

            ui.Click("Save and continue");
            ui.ExpectHeader("Your organisation");
        }
        public static void DateSet(UIContext ui, string Day, string Month, string Year, string Order)
        {
            //order is element number on page
            //on your submission page 3 present
            ui.SetXPath("(//div//label[contains(text(), 'Year')]/following-sibling::input)[" + Order + "]").To(Year);
            ui.SetXPath("(//div//label[contains(text(), 'Month')]/following-sibling::input)[" + Order + "]").To(Month);
            ui.SetXPath("(//div//label[contains(text(), 'Day')]/following-sibling::input)[" + Order + "]").To(Day);
        }

        public static void NavigateToSubmission(UIContext ui, string Organistion, string YearFromTo, bool MoreInfoRequired = false)
        {

            NavigateToMonitoringProgress(ui, Organistion, YearFromTo, MoreInfoRequired);
            
            

            ui.Click("Continue");
            ui.ExpectHeader(That.Contains, YearFromTo + " modern slavery statement for " + Organistion) ;


        }
        public static void ExpectSectors(UIContext ui, string[] Sectors)
        {
            //expect all sectors in order
            for (int i = 0; i < Sectors.Length - 1; i++)
            {
                ui.BelowLabel(The.Bottom, Sectors[i], Casing.Exact).ExpectLabel(That.Contains, Sectors[i + 1]);
            }
        }

        public static void ExpectFinancials(UIContext ui, string[] Financials)
        {
            //expect all financial options in order
            for (int i = 0; i < Financials.Length - 1; i++)
            {
                ui.BelowLabel(That.Contains, Financials[i], Casing.Exact).ExpectLabel(That.Contains, Financials[i + 1]);
            }
        }

        public static void FinancialsInteractionCheck(UIContext ui, string[] Financials)
        {
            //expect all financial options in order
            for (int i = 0; i < Financials.Length; i++)
            {
                ui.Below("What was your turnover or budget during the last financial accounting year?").ClickLabel(Financials[i]);
            }
        }

        public static void SectorsInteractionCheck(UIContext ui, string[] Financials)
        {
            //expect all financial options in order
            for (int i = 0; i < Financials.Length; i++)
            {
                //select all
                ui.ClickLabel(Financials[i]);

                //deselect all
                ui.ClickLabel(Financials[i]);
            }
        }

        public static void ExpandSection(UIContext ui, string SectionName, string SectionOrder = "1")
        {
            if (SectionOrder == "1")
            {
                ui.ClickText(The.Top, SectionName);
            }
            else if (SectionOrder == "2")
            {
                ui.ClickText(The.Bottom, SectionName);
            }

        }

        public static void ColapseSection(UIContext ui, string SectionName, string SectionOrder = "1")
        {
            if (SectionOrder == "1")
            {
                ui.ClickText(The.Top, SectionName);

            }
            else if (SectionOrder == "2")
            {
                ui.ClickText(The.Bottom, SectionName);

            }

        }

        public static void CountrySelect (UIContext ui, string Continent, string[] Countries) 
        {
            ExpandSection(ui, Continent);
            foreach (var Country in Countries)
            {
                ui.BelowHeader(Continent).ClickLabel(That.Contains, Country);
            }

           // ui.NearHeader(Continent).Expect(Countries.Length + " Selected");

        }

        public  static void ChekcboxSelector(UIContext ui, string SectionName, string[] SelectedOptions, string SectionOrder = "1", string OtherOption = null, string OtherFieldLabel = null, string OtherDetails = null, bool NeedExpand = true)
        {
            if (NeedExpand)
            {
                Submission_Helper.ExpandSection(ui, SectionName, SectionOrder);

            }

            foreach (var Option in SelectedOptions)
            {
                ui.NearHeader(SectionName).ClickLabel(Option);

                if (Option == OtherOption)
                {
                    ui.Set(OtherFieldLabel).To(OtherDetails);
                    }
            }
            //ui.NearHeader(SectionName).Expect(SelectedOptions.Length + " Selected");

            if (NeedExpand)
            {
                Submission_Helper.ColapseSection(ui, SectionName, SectionOrder);

            }

            
        }

        public static void SectionCompleteionCheck(UIContext ui, bool Completed, string Label)
        {
            if (Completed)
            {
                ui.AtXPath("//li//span[contains(., '" + Label + "')]//parent::li").Expect("Completed");
            }
            else
            {
                ui.AtXPath("//li//span[contains(., '" + Label + "')]//parent::li").Expect("Not Started");
            }
        }
    }
    }

