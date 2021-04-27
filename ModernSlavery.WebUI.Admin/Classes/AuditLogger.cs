using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using ModernSlavery.WebUI.Shared.Interfaces;

namespace ModernSlavery.WebUI.Admin.Classes
{
    public class AuditLogger
    {
        private readonly IDataRepository dataRepository;
        private readonly IHttpSession session;

        public AuditLogger(IDataRepository dataRepository, IHttpSession session)
        {
            this.dataRepository = dataRepository;
            this.session = session;
        }

        public async Task AuditChangeToOrganisationAsync(Controller controller, AuditedAction action, Organisation organisation,
            object anonymousObject)
        {
            var details = ExtractDictionaryOfDetailsFromAnonymousObject(anonymousObject);

            await AuditActionToOrganisationAsync(controller, action, organisation.OrganisationId, details);
        }

        public async Task AuditChangeToUserAsync(Controller controller, AuditedAction action, User user, object anonymousObject)
        {
            var details = ExtractDictionaryOfDetailsFromAnonymousObject(anonymousObject);

            await AuditActionToUserAsync(controller, action, user.UserId, details);
        }

        private static Dictionary<string, string> ExtractDictionaryOfDetailsFromAnonymousObject(object anonymousObject)
        {
            var details = new Dictionary<string, string>();

            var type = anonymousObject.GetType();
            var propertyInfos = type.GetProperties();

            foreach (var propertyInfo in propertyInfos)
            {
                var propertyName = propertyInfo.Name;
                var propertyValue = propertyInfo.GetValue(anonymousObject)?.ToString();

                details.Add(propertyName, propertyValue);
            }

            return details;
        }

        private async Task AuditActionToOrganisationAsync(Controller controller, AuditedAction action, long organisationId,
            Dictionary<string, string> details)
        {
            if (controller == null) throw new ArgumentNullException(nameof(controller));
            var impersonatedUserId = session["ImpersonatedUserId"].ToInt64();
            var isImpersonating = impersonatedUserId > 0;
            var originalUserId = session["OriginalUser"].ToInt64();
            var currentUser = controller.User.Identity.IsAuthenticated
                ? dataRepository.FindUser(controller?.User)
                : null;

            var originalUser = isImpersonating ? dataRepository.Get<User>(originalUserId) : currentUser;
            var impersonatedUser = dataRepository.Get<User>(impersonatedUserId);
            var organisation = dataRepository.Get<Organisation>(organisationId);

            dataRepository.Insert(
                new AuditLog
                {
                    Action = action,
                    OriginalUser = originalUser,
                    ImpersonatedUser = impersonatedUser,
                    Organisation = organisation,
                    DetailsDictionary = details
                });

            await dataRepository.SaveChangesAsync();
        }

        private async Task AuditActionToUserAsync(Controller controller, AuditedAction action, long actionTakenOnUserId,
            Dictionary<string, string> details)
        {
            if (controller == null) throw new ArgumentNullException(nameof(controller));

            var impersonatedUserId = session["ImpersonatedUserId"].ToInt64();
            var isImpersonating = impersonatedUserId > 0;
            var originalUserId = session["OriginalUser"].ToInt64();
            var currentUser = controller.User.Identity.IsAuthenticated
                ? dataRepository.FindUser(controller?.User)
                : null;

            var originalUser = isImpersonating ? dataRepository.Get<User>(originalUserId) : currentUser;
            var impersonatedUser = dataRepository.Get<User>(actionTakenOnUserId);

            if (impersonatedUser.UserId == originalUser.UserId) impersonatedUser = null;

            dataRepository.Insert(
                new AuditLog
                {
                    Action = action,
                    OriginalUser = originalUser,
                    ImpersonatedUser = impersonatedUser,
                    Organisation = null,
                    DetailsDictionary = details
                });

            await dataRepository.SaveChangesAsync();
        }
    }
}