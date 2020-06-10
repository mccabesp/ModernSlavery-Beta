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
    }
}
