using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Geeks.Pangolin;
using Geeks.Pangolin.Core.Helper;
using Geeks.Pangolin.Helper.UIContext;

namespace ModernSlavery.Testing.Helpers.Extensions
{
    public static class SubmissionHelper
    {
        public static void GroupOrSingleScreenComplete(UIContext ui, bool IsGroup = false, string OrgName = null, string Year = null)
        {
            ui.ExpectHeader("Who is your statement for?");

            if (OrgName != null && Year != null)
            {
                ui.ExpectHeader("The " + Year + " modern slavery statement for " + OrgName + " covers:");
            }

            if (IsGroup)
            {
                ui.ClickLabel("a group of organisations");
                ui.Click("Continue");
                ui.ExpectHeader("Which organisations are included in your group statement?");
            }
            else
            {
                ui.ClickLabel("a single organisation");
                ui.Click("Continue");
                ui.ExpectHeader("Your modern slavery statement");
            }
        }
    }
}
