using System;
using System.Collections.Generic;
using System.Reflection;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Extensions;
using Microsoft.AspNetCore.Mvc;
using ModernSlavery.Entities;
using ModernSlavery.Entities.Enums;
using ModernSlavery.WebUI.Shared.Interfaces;

namespace ModernSlavery.WebUI.Shared.Classes
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

        public void AuditChangeToOrganisation(Controller controller, AuditedAction action, Organisation organisation, object anonymousObject)
        {
            Dictionary<string, string> details = ExtractDictionaryOfDetailsFromAnonymousObject(anonymousObject);

            AuditActionToOrganisation(controller, action, organisation.OrganisationId, details);
        }

        public void AuditChangeToUser(Controller controller, AuditedAction action, User user, object anonymousObject)
        {
            Dictionary<string, string> details = ExtractDictionaryOfDetailsFromAnonymousObject(anonymousObject);

            AuditActionToUser(controller, action, user.UserId, details);
        }

        private static Dictionary<string, string> ExtractDictionaryOfDetailsFromAnonymousObject(object anonymousObject)
        {
            var details = new Dictionary<string, string>();

            Type type = anonymousObject.GetType();
            PropertyInfo[] propertyInfos = type.GetProperties();

            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                string propertyName = propertyInfo.Name;
                string propertyValue = propertyInfo.GetValue(anonymousObject)?.ToString();

                details.Add(propertyName, propertyValue);
            }

            return details;
        }

        private void AuditActionToOrganisation(Controller controller, AuditedAction action, long organisationId, Dictionary<string, string> details)
        {
            if (controller == null)
            {
                throw new System.ArgumentNullException(nameof(controller));
            }
            var impersonatedUserId = session["ImpersonatedUserId"].ToInt64();
            var isImpersonating = impersonatedUserId > 0;
            var originalUserId = session["OriginalUser"].ToInt64();
            var currentUser = controller.User.Identity.IsAuthenticated ? dataRepository.FindUser(controller?.User) : null;

            var originalUser = isImpersonating ? dataRepository.Get<User>(originalUserId) : currentUser;
            var impersonatedUser = dataRepository.Get<User>(impersonatedUserId);
            var organisation = dataRepository.Get<Organisation>(organisationId);

            dataRepository.Insert(
                new AuditLog {
                    Action = action,
                    OriginalUser = originalUser,
                    ImpersonatedUser = impersonatedUser,
                    Organisation = organisation,
                    DetailsDictionary = details
                });

            dataRepository.SaveChangesAsync().Wait();
        }

        private void AuditActionToUser(Controller controller, AuditedAction action, long actionTakenOnUserId, Dictionary<string, string> details)
        {
            if (controller == null)
            {
                throw new System.ArgumentNullException(nameof(controller));
            }

            var impersonatedUserId = session["ImpersonatedUserId"].ToInt64();
            var isImpersonating = impersonatedUserId > 0;
            var originalUserId = session["OriginalUser"].ToInt64();
            var currentUser = controller.User.Identity.IsAuthenticated ? dataRepository.FindUser(controller?.User) : null;

            var originalUser = isImpersonating ? dataRepository.Get<User>(originalUserId) : currentUser;
            var impersonatedUser = dataRepository.Get<User>(actionTakenOnUserId);

            if (impersonatedUser.UserId == originalUser.UserId)
            {
                impersonatedUser = null;
            }

            dataRepository.Insert(
                new AuditLog {
                    Action = action,
                    OriginalUser = originalUser,
                    ImpersonatedUser = impersonatedUser,
                    Organisation = null,
                    DetailsDictionary = details
                });

            dataRepository.SaveChangesAsync().Wait();
        }

    }
}
