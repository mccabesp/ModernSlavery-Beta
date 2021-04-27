using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using ModernSlavery.Core.Extensions;

namespace ModernSlavery.Core.Entities
{
    [Serializable]
    [DebuggerDisplay("{UserId}, {EmailAddress}, {Status}")]
    public partial class User
    {
        [NotMapped] public string Fullname => (Firstname + " " + Lastname).TrimI();

        public string GetNameAndTitle()
        {
            var fullname = Fullname;
            if (string.IsNullOrWhiteSpace(fullname)) return null;
            return string.IsNullOrWhiteSpace(JobTitle) ? fullname : $"{fullname} ({JobTitle})";
        }

        [NotMapped] public string ContactFullname => (ContactFirstName + " " + ContactLastName).TrimI();

        public string GetContactNameAndTitle()
        {
            var fullname = ContactFullname;
            if (string.IsNullOrWhiteSpace(fullname)) return null;
            return string.IsNullOrWhiteSpace(ContactJobTitle) ? fullname : $"{fullname} ({ContactJobTitle})";
        }

        [NotMapped] public bool IsVerifyEmailSent => EmailVerifySendDate>DateTime.MinValue;
        [NotMapped] public bool IsVerifiedEmail => EmailVerifiedDate>DateTime.MinValue;
        public bool IsVerificationCodeExpired(int emailVerificationExpiryHours) => EmailVerifySendDate!=null && EmailVerifySendDate.Value.AddHours(emailVerificationExpiryHours) < VirtualDateTime.Now;
        public TimeSpan GetTimeToNextVerificationResend(int emailVerificationMinResendHours) => EmailVerifySendDate==null ? TimeSpan.Zero : EmailVerifySendDate.Value.AddHours(emailVerificationMinResendHours) - VirtualDateTime.Now;

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
            StatusDate = VirtualDateTime.Now;

            UserStatuses.Add(
                new UserStatus
                {
                    User = this,
                    Status = status,
                    StatusDate = StatusDate,
                    StatusDetails = details,
                    ByUser = byUser
                });
            Status = status;
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

            var verifyCode = Encryption.Encrypt($"{UserId}:{Created.ToSmallDateTime()}", Encryption.Encodings.Base62);
            var verifyUrl = $"/sign-up/verify-email?code={verifyCode}";
            return verifyUrl;
        }

        public string GetPasswordResetUrl()
        {
            var resetCode = Encryption.Encrypt($"{UserId}:{VirtualDateTime.Now.ToSmallDateTime()}", Encryption.Encodings.Base62);
            var resetUrl = $"/register/enter-new-password?code={resetCode}";
            return resetUrl;
        }

        public override bool Equals(object obj)
        {
            var target = obj as User;
            if (target == null) return false;

            return UserId == target.UserId;
        }
    }
}