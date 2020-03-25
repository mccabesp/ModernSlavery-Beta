using System;

namespace ModernSlavery.WebUI.Account.Models.ChangeEmail
{

    [Serializable]
    public class ChangeEmailStatusViewModel
    {

        public string OldEmail { get; set; }

        public string NewEmail { get; set; }

    }

}
