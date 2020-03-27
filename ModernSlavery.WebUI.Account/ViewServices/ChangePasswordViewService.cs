using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.WebUI.Account.Interfaces;
using ModernSlavery.WebUI.Account.Models;
using ModernSlavery.WebUI.Shared.Resources;

namespace ModernSlavery.WebUI.Account.ViewServices
{
    public class ChangePasswordViewService : IChangePasswordViewService
    {
        public ChangePasswordViewService(IUserRepository userRepo, ISharedBusinessLogic sharedBusinessLogic)
        {
            _userRepository = userRepo ?? throw new ArgumentNullException(nameof(userRepo));
            _sharedBusinessLogic = sharedBusinessLogic ?? throw new ArgumentNullException(nameof(sharedBusinessLogic));
        }

        private IUserRepository _userRepository { get; }
        private ISharedBusinessLogic _sharedBusinessLogic { get; }

        public async Task<ModelStateDictionary> ChangePasswordAsync(User currentUser, string currentPassword,
            string newPassword)
        {
            var errorState = new ModelStateDictionary();

            // check users current password
            var checkPasswordResult = await _userRepository.CheckPasswordAsync(currentUser, currentPassword);
            if (checkPasswordResult == false)
            {
                errorState.AddModelError(nameof(ChangePasswordViewModel.CurrentPassword),
                    "Could not verify your current password");
                return errorState;
            }

            // prevent a user from re-using the same password
            if (currentPassword.ToLower() == newPassword.ToLower())
            {
                errorState.AddModelError(nameof(ChangePasswordViewModel.NewPassword),
                    AccountResources.ChangePasswordMustBeDifferent);
                return errorState;
            }

            // update user password
            await _userRepository.UpdatePasswordAsync(currentUser, newPassword);

            // send password change notification
            await _sharedBusinessLogic.SendEmailService.SendChangePasswordNotificationAsync(currentUser.EmailAddress);

            return errorState;
        }
    }
}