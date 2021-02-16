using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using ModernSlavery.Core.Extensions;

namespace ModernSlavery.Core.Entities
{
    [Serializable]
    [DebuggerDisplay("({Organisation}),({User})")]
    public partial class UserOrganisation
    {
        [NotMapped] public bool HasPINCode => !string.IsNullOrWhiteSpace(PIN) || !string.IsNullOrWhiteSpace(PINHash);
        [NotMapped] public bool IsPINCodeSent => PINSentDate > DateTime.MinValue;
        [NotMapped] public bool IsRegisteredOK => PINConfirmedDate > DateTime.MinValue;
        public bool IsPINCodeExpired(int pinCodeExpiryDays) => PINSentDate != null && PINSentDate.Value.AddDays(pinCodeExpiryDays) < VirtualDateTime.Now;
        public TimeSpan GetTimeToNextPINResend(int pinCodeMinResendDays) => PINSentDate == null ? TimeSpan.Zero : PINSentDate.Value.AddDays(pinCodeMinResendDays) - VirtualDateTime.Now;

        public bool IsPINAttemptsExceeded(int maxPinAttempts) => ConfirmAttempts >= maxPinAttempts;
        public TimeSpan GetTimeToNextPINAttempt(int pinLockoutMinutes) => ConfirmAttemptDate == null ? TimeSpan.Zero : ConfirmAttemptDate.Value.AddMinutes(pinLockoutMinutes) -VirtualDateTime.Now;

        public bool IsCorrectPin(string pin)
        {
            if (string.IsNullOrWhiteSpace(pin)) return false;

            pin = pin.Trim().ToUpper();
            if (!string.IsNullOrWhiteSpace(pin) && PIN == pin) return true;

            pin = Crypto.GetSHA512Checksum(pin);
            if (PINHash == pin) return true;

            return false;
        }

        public string GetReviewCode()
        {
            return Encryption.Encrypt($"{UserId}:{OrganisationId}:{VirtualDateTime.Now.ToSmallDateTime()}", Encryption.Encodings.Base62);
        }

        public IEnumerable<UserOrganisation> GetAssociatedUsers()
        {
            return Organisation.UserOrganisations.Where(uo =>
                uo.OrganisationId == OrganisationId
                && uo.UserId != UserId
                && uo.PINConfirmedDate != null
                && uo.User.Status == UserStatuses.Active);
        }
    }
}