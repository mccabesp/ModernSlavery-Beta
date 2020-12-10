using System;
using System.ComponentModel.DataAnnotations;
using ModernSlavery.WebUI.Shared.Models;

namespace ModernSlavery.WebUI.Registration.Models
{
    [Serializable]
    public class AddContactViewModel
    {
        [MaxLength(50)]
        [Required(AllowEmptyStrings = false)]
        public string ContactFirstName { get; set; }

        [MaxLength(50)]
        [Required(AllowEmptyStrings = false)]
        public string ContactLastName { get; set; }

        [MaxLength(50)]
        [Required(AllowEmptyStrings = false)]
        public string ContactJobTitle { get; set; }

        [Required(AllowEmptyStrings = false)]
        [DataType(DataType.EmailAddress)]
        public string ContactEmailAddress { get; set; }

        public string EmailAddress { get; set; }

        [Required(AllowEmptyStrings = false)]
        [Phone]
        [MaxLength(20)]
        public string ContactPhoneNumber { get; set; }
    }
}