using System;
using System.ComponentModel.DataAnnotations;

namespace ModernSlavery.WebUI.Shared.Models
{
    [Serializable]
    public class PrivacyStatementModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "You must accept this statement in order to continue")]
        public string Accept { get; set; }
    }
}