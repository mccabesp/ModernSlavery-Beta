using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ModernSlavery.WebUI.Areas.Account.Abstractions;
using ModernSlavery.WebUI.Areas.Account.ViewModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using ModernSlavery.BusinessLogic;
using ModernSlavery.BusinessLogic.Registration;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.WebUI.Areas.Account.ViewModels.ChangePassword;

namespace ModernSlavery.WebUI.Areas.Account.ViewServices
{

    public class CloseAccountViewService : ICloseAccountViewService
    {

        public CloseAccountViewService(
            IUserRepository userRepository,
            IRegistrationBusinessLogic registrationBusinessLogic,
            ILogger<CloseAccountViewService> logger,
            ISendEmailService sendEmailService,
            ISharedBusinessLogic sharedBusinessLogic)
        {
            UserRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            RegistrationBusinessLogic = registrationBusinessLogic ?? throw new ArgumentNullException(nameof(registrationBusinessLogic));
            Logger = logger;
            SendEmailService = sendEmailService;
        }


        private IUserRepository UserRepository { get; }
        private IRegistrationBusinessLogic RegistrationBusinessLogic { get; }
        private ILogger<CloseAccountViewService> Logger { get; }
        private ISendEmailService SendEmailService { get; }
        private readonly ISharedBusinessLogic _sharedBusinessLogic;

        public async Task<ModelStateDictionary> CloseAccountAsync(User userToRetire, string currentPassword, User actionByUser)
        {
            var errorState = new ModelStateDictionary();

            // ensure the user has entered their password
            bool checkPasswordResult = await UserRepository.CheckPasswordAsync(userToRetire, currentPassword);
            if (checkPasswordResult == false)
            {
                errorState.AddModelError(nameof(ChangePasswordViewModel.CurrentPassword), "Could not verify your current password");
                return errorState;
            }

            //Save the list of registered organisations
            List<Organisation> userOrgs = userToRetire.UserOrganisations.Select(uo => uo.Organisation).Distinct().ToList();

            // aggregated save
            await UserRepository.BeginTransactionAsync(
                async () => {
                    try
                    {
                        // update retired user registrations 
                        await RegistrationBusinessLogic.RemoveRetiredUserRegistrationsAsync(userToRetire, actionByUser);

                        // retire user
                        await UserRepository.RetireUserAsync(userToRetire);

                        // commit
                        UserRepository.CommitTransaction();
                    }
                    catch (Exception ex)
                    {
                        UserRepository.RollbackTransaction();
                        Logger.LogWarning(
                            ex,
                            "Failed to retire user {UserId}. Action by user {ActionByUserId}",
                            userToRetire.UserId,
                            actionByUser.UserId);
                        throw;
                    }
                });

            if (!userToRetire.EmailAddress.StartsWithI(_sharedBusinessLogic.SharedOptions.TestPrefix))
            {
                // Create the close account notification to user
                var sendEmails = new List<Task>();
                bool testEmail = !_sharedBusinessLogic.SharedOptions.IsProduction();
                sendEmails.Add(SendEmailService.SendAccountClosedNotificationAsync(userToRetire.EmailAddress, testEmail));

                //Create the notification to GEO for each newly orphaned organisation
                userOrgs.Where(org => org.GetIsOrphan())
                    .ForEach(org => sendEmails.Add(SendEmailService.SendGEOOrphanOrganisationNotificationAsync(org.OrganisationName, testEmail)));

                //Send all the notifications in parallel
                await Task.WhenAll(sendEmails);
            }

            return errorState;
        }

    }

}
