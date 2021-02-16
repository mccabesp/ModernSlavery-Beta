using System;
using System.ComponentModel.DataAnnotations;
using ModernSlavery.WebUI.Shared.Classes.Attributes;

namespace ModernSlavery.WebUI.Registration.Models
{
    [Serializable]
    public class AddContactViewModel
    {
        [MaxLength(50)]
        [Required(AllowEmptyStrings = false)]
        [Text] 
        public string ContactFirstName { get; set; }

        [MaxLength(50)]
        [Required(AllowEmptyStrings = false)]
        [Text] 
        public string ContactLastName { get; set; }

        [MaxLength(50)]
        [Required(AllowEmptyStrings = false)]
        [Text] 
        public string ContactJobTitle { get; set; }

        [Required(AllowEmptyStrings = false)]
        [EmailAddress]
        public string ContactEmailAddress { get; set; }

        [Required(AllowEmptyStrings = false)]
        [Phone]
        [MaxLength(20)]
        public string ContactPhoneNumber { get; set; }
    }
}