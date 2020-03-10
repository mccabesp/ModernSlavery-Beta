using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using ModernSlavery.Entities.Enums;
using ModernSlavery.Extensions;
using Microsoft.Extensions.Configuration;
using ModernSlavery.SharedKernel.Options;

namespace ModernSlavery.Entities
{

    [Serializable]
    [DebuggerDisplay("{UserId}, {EmailAddress}, {Status}")]
    public partial class User
    {
        private GlobalOptions GlobalOptions;

        private string AdminEmails => GlobalOptions.AdminEmails;
        private string SuperAdminEmails => GlobalOptions.SuperAdminEmails;
        private string DatabaseAdminEmails => GlobalOptions.DatabaseAdminEmails;

        public User(GlobalOptions globalOptions)
        {
            GlobalOptions = globalOptions;
        }

        [NotMapped]
        public bool EncryptEmails => GlobalOptions.EncryptEmails;

        [NotMapped]
        public string Fullname => (Firstname + " " + Lastname).TrimI();

        [NotMapped]
        public string ContactFullname => (ContactFirstName + " " + ContactLastName).TrimI();

        [NotMapped]
        public TimeSpan LockRemaining =>
            LoginDate == null || LoginAttempts < GlobalOptions.MaxLoginAttempts
                ? TimeSpan.Zero
                : LoginDate.Value.AddMinutes(GlobalOptions.LockoutMinutes) - VirtualDateTime.Now;

        [NotMapped]
        public bool SendUpdates
        {
            get => GetSetting(UserSettingKeys.SendUpdates) == "True";
            set => SetSetting(UserSettingKeys.SendUpdates, value.ToString());
        }

        [NotMapped]
        public bool AllowContact
        {
            get => GetSetting(UserSettingKeys.AllowContact) == "True";
            set => SetSetting(UserSettingKeys.AllowContact, value.ToString());
        }

        [NotMapped]
        public DateTime? AcceptedPrivacyStatement
        {
            get
            {
                string value = GetSetting(UserSettingKeys.AcceptedPrivacyStatement);
                if (value == null)
                {
                    return null;
                }

                return DateTime.Parse(value);
            }
            set => SetSetting(UserSettingKeys.AcceptedPrivacyStatement, value.HasValue ? value.Value.ToString() : null);
        }

        public bool IsAdministrator()
        {
            if (!EmailAddress.IsEmailAddress())
            {
                throw new ArgumentException("Bad email address");
            }

            if (string.IsNullOrWhiteSpace(AdminEmails))
            {
                throw new ArgumentException("Missing AdminEmails from web.config");
            }

            return EmailAddress.LikeAny(AdminEmails.SplitI(";"));
        }

        public bool IsSuperAdministrator()
        {
            if (!EmailAddress.IsEmailAddress())throw new ArgumentException("Bad email address");

            if (string.IsNullOrWhiteSpace(SuperAdminEmails))throw new ArgumentException("Missing SuperAdminEmails from web.config");

            return EmailAddress.LikeAny(SuperAdminEmails.SplitI(";"));
        }

        public bool IsDatabaseAdministrator()
        {
            if (!EmailAddress.IsEmailAddress())throw new ArgumentException("Bad email address");

            if (string.IsNullOrWhiteSpace(DatabaseAdminEmails))
                return IsSuperAdministrator();

            return EmailAddress.LikeAny(DatabaseAdminEmails.SplitI(";"));
        }

        /// <summary>
        ///     Determines if the user is the only registration of any of their UserOrganisations
        /// </summary>
        public bool IsSoleUserOfOneOrMoreOrganisations()
        {
            return UserOrganisations.Any(uo => uo.GetAssociatedUsers().Any() == false);
        }

        public void SetStatus(UserStatuses status, User byUser, string details = null)
        {
            //ByUser must be an object and not the id itself otherwise a foreign key exception is thrown with EF core due to being unable to resolve the ByUserId
            if (status == Status && details == StatusDetails)
            {
                return;
            }

            UserStatuses.Add(
                new UserStatus {
                    User = this,
                    Status = status,
                    StatusDate = VirtualDateTime.Now,
                    StatusDetails = details,
                    ByUser = byUser
                });
            Status = status;
            StatusDate = VirtualDateTime.Now;
            StatusDetails = details;
        }

        public string GetSetting(UserSettingKeys key)
        {
            UserSetting setting = UserSettings.FirstOrDefault(s => s.Key == key);

            if (setting != null && !string.IsNullOrWhiteSpace(setting.Value))
            {
                return setting.Value;
            }

            return null;
        }

        public void SetSetting(UserSettingKeys key, string value)
        {
            UserSetting setting = UserSettings.FirstOrDefault(s => s.Key == key);
            if (string.IsNullOrWhiteSpace(value))
            {
                if (setting != null)
                {
                    UserSettings.Remove(setting);
                }
            }
            else if (setting == null)
            {
                UserSettings.Add(new UserSetting(key, value));
            }
            else if (setting.Value != value)
            {
                setting.Value = value;
                setting.Modified = VirtualDateTime.Now;
            }
        }

        public string GetVerifyUrl()
        {
            if (EmailVerifiedDate != null)
            {
                return null;
            }

            string verifyCode = Encryption.EncryptQuerystring(UserId + ":" + Created.ToSmallDateTime());
            string verifyUrl = $"/register/verify-email?code={verifyCode}";
            return verifyUrl;
        }

        public string GetPasswordResetUrl()
        {
            string resetCode = Encryption.EncryptQuerystring(UserId + ":" + VirtualDateTime.Now.ToSmallDateTime());
            string resetUrl = $"/register/enter-new-password?code={resetCode}";
            return resetUrl;
        }

        public override bool Equals(object obj)
        {
            // Check for null values and compare run-time types.
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var target = (User) obj;
            return UserId == target.UserId;
        }

    }

}
