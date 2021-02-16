using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.WebUI.Account.Interfaces;
using ModernSlavery.WebUI.Account.Models;

namespace ModernSlavery.WebUI.Account.ViewServices
{
    public class CloseAccountViewService : ICloseAccountViewService
    {
        public CloseAccountViewService(
            IUserRepository userRepository,
            IRegistrationBusinessLogic registrationBusinessLogic,
            IOrganisationBusinessLogic organisationBusinessLogic,
            ILogger<CloseAccountViewService> logger,
            ISendEmailService sendEmailService,
            ISharedBusinessLogic sharedBusinessLogic)
        {
            UserRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            RegistrationBusinessLogic = registrationBusinessLogic ??
                                        throw new ArgumentNullException(nameof(registrationBusinessLogic));
            _organisationBusinessLogic = organisationBusinessLogic;
            Logger = logger;
            SendEmailService = sendEmailService;
            _sharedBusinessLogic = sharedBusinessLogic;
        }


        private readonly ISharedBusinessLogic _sharedBusinessLogic;
        private IUserRepository UserRepository { get; }
        private IRegistrationBusinessLogic RegistrationBusinessLogic { get; }
        private readonly IOrganisationBusinessLogic _organisationBusinessLogic;
        private ILogger<CloseAccountViewService> Logger { get; }
        private ISendEmailService SendEmailService { get; }

        public async Task<ModelStateDictionary> CloseAccountAsync(User userToRetire, string currentPassword,
            User actionByUser)
        {
            var errorState = new ModelStateDictionary();

            // ensure the user has entered their password
            var checkPasswordResult = await UserRepository.CheckPasswordAsync(userToRetire, currentPassword);
            if (checkPasswordResult == false)
            {
                errorState.AddModelError(nameof(ChangePasswordViewModel.CurrentPassword),
                    "Could not verify your current password");
                return errorState;
            }

            //Save the list of registered organisations
            var userOrgs = userToRetire.UserOrganisations.Select(uo => uo.Organisation).Distinct().ToList();

            // aggregated save
            await UserRepository.ExecuteTransactionAsync(
                async () =>
                {
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

            // Create the close account notification to user
            var sendEmails = new List<Task>();
            sendEmails.Add(
                SendEmailService.SendAccountClosedNotificationAsync(userToRetire.EmailAddress));

            //Create the notification to GEO for each newly orphaned organisation
            userOrgs.Where(org => _organisationBusinessLogic.GetOrganisationIsOrphan(org))
                .ForEach(org => sendEmails.Add(SendEmailService.SendMsuOrphanOrganisationNotificationAsync(org.OrganisationName)));

            //Send all the notifications in parallel
            await Task.WhenAll(sendEmails);

            return errorState;
        }
    }
}