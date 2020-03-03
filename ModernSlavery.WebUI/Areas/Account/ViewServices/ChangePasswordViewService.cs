using System;
using System.Threading.Tasks;
using ModernSlavery.BusinessLogic.Abstractions;
using ModernSlavery.WebUI.Areas.Account.Abstractions;
using ModernSlavery.WebUI.Areas.Account.Resources;
using ModernSlavery.WebUI.Areas.Account.ViewModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using ModernSlavery.Entities;
using ModernSlavery.BusinessLogic;

namespace ModernSlavery.WebUI.Areas.Account.ViewServices
{

    public class ChangePasswordViewService : IChangePasswordViewService
    {

        public ChangePasswordViewService(IUserRepository userRepo, ICommonBusinessLogic commonBusinessLogic)
        {
            _userRepository = userRepo ?? throw new ArgumentNullException(nameof(userRepo));
            _commonBusinessLogic = commonBusinessLogic ?? throw new ArgumentNullException(nameof(commonBusinessLogic));
        }

        private IUserRepository _userRepository { get; }
        private ICommonBusinessLogic _commonBusinessLogic { get; }

        public async Task<ModelStateDictionary> ChangePasswordAsync(User currentUser, string currentPassword, string newPassword)
        {
            var errorState = new ModelStateDictionary();

            // check users current password
            bool checkPasswordResult = await _userRepository.CheckPasswordAsync(currentUser, currentPassword);
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
            await _userRepository.UpdatePasswordAsync(currentUser, newPassword);

            // send password change notification
            await _commonBusinessLogic.SendEmailService.SendChangePasswordNotificationAsync(currentUser.EmailAddress);

            return errorState;
        }

    }

}
