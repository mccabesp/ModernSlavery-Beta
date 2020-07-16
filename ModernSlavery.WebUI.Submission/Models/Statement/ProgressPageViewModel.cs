using System;
using System.Collections.Generic;
using System.Text;

namespace ModernSlavery.WebUI.Submission.Models
{
    public class ProgressPageViewModel
    {
        public int Year { get; set; }
        public string OrganisationIdentifier { get; set; }

        public bool IncludesMeasuringProgress { get; set; }

        public string ProgressMeasures { get; set; }

        public string KeyAchievements { get; set; }

        public Presenters.NumberOfYearsOfStatements NumberOfYearsOfStatements { get; set; }
    }
}
