using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pangolin;
using Pangolin.Helper.UIContext;
namespace Modern_Slavery_Reporting_UI_Tests

{
    class Submission_Helper
    {
        

        public static void NavigateToTraining(UIContext ui, string Organistion, string Year)
        {
            ui.Click("Mange Organisations");
            ui.ExpectHeader("Select an organisation");

            ui.Click("Organisation");
            ui.AtRow(Year).Click("Draft report");
            ui.ExpectHeader("Your modern slavery statement");

            ui.Click("Save and continue");
            ui.ExpectHeader("Six areas of modern slavery statement");

            ui.Click("Save and continue");
            ui.ExpectHeader("Your organisation");

            ui.Click("Save and continue");
            ui.ExpectHeader("Supply chain risk");

            ui.Click("Save and continue");
            ui.ExpectHeader("Policies");

            ui.Click("Save and continue");
            ui.ExpectHeader("Due diligence");

            ui.Click("Save and continue");
            ui.ExpectHeader("Training");
        }
        public static void NavigateToSectors(UIContext ui, string Organistion, string Year) {
            ui.Click("Mange Organisations");
            ui.ExpectHeader("Select an organisation");

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
            ui.SetXPath("(//div//label[contains(text(), 'Year')]/following-sibling::input)[" + Order + "])").To(Year);
            ui.SetXPath("(//div//label[contains(text(), 'Month')]/following-sibling::input)[" + Order + "])").To(Month);
            ui.SetXPath("(//div//label[contains(text(), 'Day')]/following-sibling::input)[" + Order + "])").To(Day);
        }
        public static void ExpectSectors(UIContext ui, string[] Sectors)
        {
            //expect all sectors in order
            for (int i = 0; i < Sectors.Length - 1; i++)
            {
                ui.BelowLabel(Sectors[i]).ExpectLabel(Sectors[i + 1]);
            }        
    }

        public static void ExpectFinancials(UIContext ui, string[] Financials)
        {
            //expect all financial options in order
            for (int i = 0; i < Financials.Length - 1; i++)
            {
                ui.BelowLabel(Financials[i]).ExpectLabel(Financials[i + 1]);
            }
        }

        public static void FinancialsInteractionCheck(UIContext ui, string[] Financials)
        {
            //expect all financial options in order
            for (int i = 0; i < Financials.Length; i++)
            {
                ui.Set("What was your turnover or budget during the last financial accounting year?").To(Financials[i]);
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
    }
}
