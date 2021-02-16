using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using ModernSlavery.WebUI.Shared.Classes.Attributes;

namespace ModernSlavery.WebUI.Admin.Models
{
    [Serializable]
    public class ManualChangesViewModel
    {
        [IgnoreText] public string LastTestedInput { get; set; }
        [IgnoreText] public string LastTestedCommand { get; set; }
        [IgnoreText] public string Command { get; set; }
        [Text] public string Parameters { get; set; }
        [BindNever] public string Results { get; set; }
        [Text] public string Comment { get; set; }
        public bool Tested { get; set; }
        [BindNever] public string SuccessMessage { get; set; }
    }
}