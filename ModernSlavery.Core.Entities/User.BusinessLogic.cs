using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.SharedKernel.Options;

namespace ModernSlavery.Core.Entities
{
    [Serializable]
    [DebuggerDisplay("{UserId}, {EmailAddress}, {Status}")]
    public partial class User
    {
        private SharedOptions SharedOptions;

        public User(SharedOptions sharedOptions) : this()
        {
            SharedOptions = sharedOptions;
        }

        private string AdminEmails => SharedOptions.AdminEmails;
        private string SuperAdminEmails => SharedOptions.SuperAdminEmails;
        private string DatabaseAdminEmails => SharedOptions.DatabaseAdminEmails;

        [NotMapped] public bool EncryptEmails => SharedOptions.EncryptEmails;

        [NotMapped] public string Fullname => (Firstname + " " + Lastname).TrimI();

        [NotMapped] public string ContactFullname => (ContactFirstName + " " + ContactLastName).TrimI();

        [NotMapped]
        public TimeSpan LockRemaining =>
            LoginDate == null || LoginAttempts < SharedOptions.MaxLoginAttempts
                ? TimeSpan.Zero
                : LoginDate.Value.AddMinutes(SharedOptions.LockoutMinutes) - VirtualDateTime.Now;

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
                var value = GetSetting(UserSettingKeys.AcceptedPrivacyStatement);
                if (value == null) return null;

                return DateTime.Parse(value);
            }
            set => SetSetting(UserSettingKeys.AcceptedPrivacyStatement, value.HasValue ? value.Value.ToString() : null);
        }

        public bool IsAdministrator()
        {
            if (!EmailAddress.IsEmailAddress()) throw new ArgumentException("Bad email address");

            if (string.IsNullOrWhiteSpace(AdminEmails))
                throw new ArgumentException("Missing AdminEmails from web.config");

            return EmailAddress.LikeAny(AdminEmails.SplitI(";"));
        }

        public bool IsSuperAdministrator()
        {
            if (!EmailAddress.IsEmailAddress()) throw new ArgumentException("Bad email address");

            if (string.IsNullOrWhiteSpace(SuperAdminEmails))
                throw new ArgumentException("Missing SuperAdminEmails from web.config");

            return EmailAddress.LikeAny(SuperAdminEmails.SplitI(";"));
        }

        public bool IsDatabaseAdministrator()
        {
            if (!EmailAddress.IsEmailAddress()) throw new ArgumentException("Bad email address");

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
            if (status == Status && details == StatusDetails) return;

            UserStatuses.Add(
                new UserStatus
                {
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
            var setting = UserSettings.FirstOrDefault(s => s.Key == key);

            if (setting != null && !string.IsNullOrWhiteSpace(setting.Value)) return setting.Value;

            return null;
        }

        public void SetSetting(UserSettingKeys key, string value)
        {
            var setting = UserSettings.FirstOrDefault(s => s.Key == key);
            if (string.IsNullOrWhiteSpace(value))
            {
                if (setting != null) UserSettings.Remove(setting);
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
            if (EmailVerifiedDate != null) return null;

            var verifyCode = Encryption.EncryptQuerystring(UserId + ":" + Created.ToSmallDateTime());
            var verifyUrl = $"/register/verify-email?code={verifyCode}";
            return verifyUrl;
        }

        public string GetPasswordResetUrl()
        {
            var resetCode = Encryption.EncryptQuerystring(UserId + ":" + VirtualDateTime.Now.ToSmallDateTime());
            var resetUrl = $"/register/enter-new-password?code={resetCode}";
            return resetUrl;
        }

        public override bool Equals(object obj)
        {
            // Check for null values and compare run-time types.
            if (obj == null || GetType() != obj.GetType()) return false;

            var target = (User) obj;
            return UserId == target.UserId;
        }
    }
}