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
                ui.ExpectHeader(That.Contains, "Which organisations are included in your group statement?");
            }
            else
            {
                ui.ClickLabel("a single organisation");
                ui.Click("Continue");
                ui.ExpectHeader("Your modern slavery statement");
            }
        }

        public static void MoreInformationRequiredComplete(UIContext ui, bool WasRequired, string DeadlineYear = "2019", string OrgName = null)
        {

         ui.Expect("We need more information");
            if (OrgName.HasValue())
            {
                ui.Expect(OrgName);
            }

            ui.Expect("Was your organisation required to publish a modern slavery statement on its website in " + DeadlineYear + "?");

            if (WasRequired)
            {
                ui.ClickLabel("Yes");
            }
            else
            {
                ui.ClickLabel("No");
            }

            ui.Expect(What.Contains, "If you don't know whether your organisation was required to publish a modern slavery statement,");
            ui.ExpectLink(That.Contains, "refer to the guidance");

            ui.Click("Continue");

            //todo fix this expect
            //ui.Expect(What.Contains, "You've confirmed your organisation was required to publish a modern slavery statement on its website in "+ DeadlineYear+".");
            if (OrgName.HasValue())
            {
                ui.ExpectHeader(OrgName);
            }


            ui.Click("Continue");
            ui.ExpectHeader(That.Contains, "Manage your modern slavery statement submissions");
        }
    }
}
