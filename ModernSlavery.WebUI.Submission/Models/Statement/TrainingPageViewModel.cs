using System;
using System.Collections.Generic;
using System.Text;

namespace ModernSlavery.WebUI.Submission.Models
{
    public class TrainingPageViewModel
    {
        public int Year { get; set; }
        public string OrganisationIdentifier { get; set; }

        public IList<TrainingViewModel> AllTraining { get; set; }
        public IList<TrainingViewModel> Training { get; set; }

        public string OtherTraining { get; set; }

        public class TrainingViewModel
        {
            public short Id { get; set; }

            public string Description { get; set; }
        }
    }
}
