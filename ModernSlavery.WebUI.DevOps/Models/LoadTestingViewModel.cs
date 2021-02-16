using System;
using System.Collections.Generic;
using System.Text;
using ModernSlavery.WebUI.Shared.Classes.Attributes;
using ModernSlavery.WebUI.Shared.Classes.ViewModelBinder;
using ModernSlavery.WebUI.Shared.Models;

namespace ModernSlavery.WebUI.DevOps.Models
{
    [SessionViewState]
    public class LoadTestingViewModel:BaseViewModel
    {
        public HashSet<string> Environments = new HashSet<string>();

        [IgnoreText]
        public int EnvironmentIndex { get; set; }

        [IgnoreText]
        public string Command { get; set; }
    }
}
