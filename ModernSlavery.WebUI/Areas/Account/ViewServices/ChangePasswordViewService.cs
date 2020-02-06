using System;
using System.Threading.Tasks;
using ModernSlavery.BusinessLogic.Account.Abstractions;
using ModernSlavery.Database;
using ModernSlavery.WebUI.Areas.Account.Abstractions;
using ModernSlavery.WebUI.Areas.Account.Resources;
using ModernSlavery.WebUI.Areas.Account.ViewModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ModernSlavery.WebUI.Areas.Account.ViewServices
{

    public class ChangePasswordViewService : IChangePasswordViewService
    {

        public ChangePasswordViewService(IUserRepository userRepo)
        {
            UserRepository = userRepo ?? throw new ArgumentNullException(nameof(userRepo));
        }

        private IUserRepository UserRepository { get; }

        public async Task<ModelStateDictionary> ChangePasswordAsync(User currentUser, string currentPassword, string newPassword)
        {
            var errorState = new ModelStateDictionary();

            // check users current password
            bool checkPasswordResult = await UserRepository.CheckPasswordAsync(currentUser, currentPassword);
            if (checkPasswordResult == false)
            {
                errorState.AddModelError(nameof(ChangePasswordViewModel.CurrentPassword), "Could not verify your current password");
                return errorState;
            }

            // prevent a user from re-using the same password
            if (currentPassword.ToLower() == newPassword.ToLower())
            {
                errorState.AddModelError(nameof(ChangePasswordViewModel.NewPassword), AccountResources.ChangePasswordMustBeDifferent);
                return errorState;
            }

            // update user password
            await UserRepository.UpdatePasswordAsync(currentUser, newPassword);

            // send password change notification
            await Emails.SendChangePasswordNotificationAsync(currentUser.EmailAddress);

            return errorState;
        }

    }

}
