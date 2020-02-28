using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;
using ModernSlavery.Entities;
using ModernSlavery.Extensions;
using ModernSlavery.Entities.Enums;

namespace ModernSlavery.Entities
{
    public partial class User
    {

        public User()
        {
            AddressStatus = new HashSet<AddressStatus>();
            OrganisationStatus = new HashSet<OrganisationStatus>();
            ReturnStatus = new HashSet<ReturnStatus>();
            UserOrganisations = new HashSet<UserOrganisation>();
            UserSettings = new HashSet<UserSetting>();
            UserStatusesByUser = new HashSet<UserStatus>();
            UserStatuses = new HashSet<UserStatus>();
        }

        public long UserId { get; set; }
        public string JobTitle { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }

        [NotMapped]
        public string _EmailAddress;

        public string EmailAddress
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_EmailAddress))
                {
                    try
                    {
                        return Encryption.DecryptData(_EmailAddress);
                    }
                    catch (CryptographicException) { }
                }

                return _EmailAddress;
            }
            set
            {
                if (!string.IsNullOrWhiteSpace(value) && EncryptEmails)
                {
                    _EmailAddress = Encryption.EncryptData(value.ToLower());
                }
                else
                {
                    _EmailAddress = value;
                }
            }
        }

        public string ContactJobTitle { get; set; }
        public string ContactFirstName { get; set; }
        public string ContactLastName { get; set; }
        public string ContactOrganisation { get; set; }

        private string _ContactEmailAddress;
        public string ContactEmailAddress
        {
            get => string.IsNullOrWhiteSpace(_ContactEmailAddress) ? _ContactEmailAddress : Encryption.DecryptData(_ContactEmailAddress);
            set
            {
                if (!string.IsNullOrWhiteSpace(value) && EncryptEmails)
                {
                    _ContactEmailAddress = Encryption.EncryptData(value);
                }
                else
                {
                    _ContactEmailAddress = value;
                }
            }
        }

        public string ContactPhoneNumber { get; set; }
        public string PasswordHash { get; set; }
        public string Salt { get; set; }
        public HashingAlgorithm HashingAlgorithm { get; set; }
        public string EmailVerifyHash { get; set; }
        public DateTime? EmailVerifySendDate { get; set; }
        public DateTime? EmailVerifiedDate { get; set; }
        public UserStatuses Status { get; set; }

        public DateTime StatusDate { get; set; } = VirtualDateTime.Now;
        public string StatusDetails { get; set; }
        public int LoginAttempts { get; set; }
        public DateTime? LoginDate { get; set; }
        public DateTime? ResetSendDate { get; set; }
        public int ResetAttempts { get; set; }

        /// <summary>
        ///     The last time the user attempted to verify their email address but failed
        /// </summary>
        public DateTime? VerifyAttemptDate { get; set; }

        /// <summary>
        ///     How many times the user attempted to verify their email address but failed
        /// </summary>
        public int VerifyAttempts { get; set; }

        public DateTime Created { get; set; } = VirtualDateTime.Now;
        public DateTime Modified { get; set; } = VirtualDateTime.Now;

        public virtual ICollection<AddressStatus> AddressStatus { get; set; }
        public virtual ICollection<OrganisationStatus> OrganisationStatus { get; set; }
        public virtual ICollection<ReturnStatus> ReturnStatus { get; set; }
        public virtual ICollection<UserOrganisation> UserOrganisations { get; set; }
        public virtual ICollection<UserSetting> UserSettings { get; set; }
        public virtual ICollection<UserStatus> UserStatusesByUser { get; set; }
        public virtual ICollection<UserStatus> UserStatuses { get; set; }

    }
}
