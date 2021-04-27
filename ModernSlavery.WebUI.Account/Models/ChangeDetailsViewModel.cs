using System;
using System.ComponentModel.DataAnnotations;
using ModernSlavery.WebUI.Shared.Classes.Attributes;
using ModernSlavery.WebUI.Shared.Resources;

namespace ModernSlavery.WebUI.Account.Models
{
    [Serializable]
    [DefaultResource(typeof(AccountResources))]
    public class ChangeDetailsViewModel
    {
        [StringLength(50, ErrorMessageResourceName = nameof(AccountResources.FirstNameLength))]
        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = nameof(AccountResources.FirstNameRequired))]
        [Display(Name = nameof(FirstName))]
        [Text] 
        public string FirstName { get; set; }

        [StringLength(50, ErrorMessageResourceName = nameof(AccountResources.LastNameLength))]
        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = nameof(AccountResources.LastNameRequired))]
        [Display(Name = nameof(LastName))]
        [Text] 
        public string LastName { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = nameof(AccountResources.JobTitleRequired))]
        [StringLength(50, ErrorMessageResourceName = nameof(AccountResources.JobTitleLength))]
        [Display(Name = nameof(JobTitle))]
        [Text] 
        public string JobTitle { get; set; }

        [MaxLength(20, ErrorMessageResourceName = nameof(AccountResources.ContactPhoneNumberLength))]
        [Display(Name = nameof(ContactPhoneNumber))]
        [Phone]
        public string ContactPhoneNumber { get; set; }

        [Display(Name = nameof(SendUpdates))] public bool SendUpdates { get; set; }

        [Display(Name = nameof(AllowContact))] public bool AllowContact { get; set; }

        public override bool Equals(object obj)
        {
            // Check for null values and compare run-time types.
            if (obj == null || GetType() != obj.GetType()) return false;

            var target = (ChangeDetailsViewModel) obj;

            return target.FirstName == FirstName
                   && target.LastName == LastName
                   && target.JobTitle == JobTitle
                   && target.ContactPhoneNumber == ContactPhoneNumber
                   && target.SendUpdates == SendUpdates
                   && target.AllowContact == AllowContact;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                FirstName,
                LastName,
                JobTitle,
                ContactPhoneNumber,
                SendUpdates,
                AllowContact);
        }
    }
}