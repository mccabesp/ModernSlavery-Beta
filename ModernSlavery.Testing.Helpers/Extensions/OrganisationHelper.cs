using EFCore.BulkExtensions;
using Microsoft.Extensions.Hosting;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using ModernSlavery.BusinessDomain.Registration;
using ModernSlavery.BusinessDomain.Shared.Interfaces;

/// <summary>
/// TODO: Use Business Logicfunctionality
/// </summary>

namespace ModernSlavery.Testing.Helpers.Extensions
{
    public static class OrganisationHelper
    {
        public static Organisation GetOrganisation(this IHost host, string organisationName)
        {
            var dataRepository = host.GetDataRepository();
            return dataRepository.GetAll<Organisation>().SingleOrDefault(o => o.OrganisationName == organisationName);
        }

        public static IEnumerable<Organisation> ListOrganisations(this IHost host)
        {
            var dataRepository = host.GetDataRepository();
            return dataRepository.GetAll<Organisation>();
        }

        /// <summary>
        /// Creates a registration for a user with an organisation
        /// </summary>
        /// <param name="host">The webhost</param>
        /// <param name="organisationName">The name of the organisation</param>
        /// <param name="firstName">The first name of the user</param>
        /// <param name="lastName">The last name of the user</param>
        /// <param name="pin">If empty the registration is activated. Otherwise unactivated and pin set to this value</param>
        /// <param name="pinSendDate">When pin not null this value marks when the pin was sent - set it to an old value to expire the pin</param>
        /// <returns></returns>
        public static async Task<UserOrganisation> RegisterUserOrganisationAsync(this IHost host, string organisationName, string firstName, string lastName, string pin=null, DateTime? pinSendDate=null)
        {
            var dataRepository = host.GetDataRepository();
            var organisation = dataRepository.GetAll<Organisation>().SingleOrDefault(o => o.OrganisationName == organisationName);
            var organisationId = organisation.OrganisationId;

            var user = dataRepository.GetAll<User>().SingleOrDefault(u => u.Firstname == firstName && u.Lastname == lastName);
            var userId = user.UserId;
            var email = user.EmailAddress.ToLower();

            var userOrganisation = dataRepository.Get<UserOrganisation>(userId, organisationId);
            if (userOrganisation == null)
            {
                userOrganisation = new UserOrganisation { UserId = userId, OrganisationId = organisationId };
                dataRepository.Insert(userOrganisation);
            }
            if (string.IsNullOrWhiteSpace(pin))
            {
                pin = "ABCDEFGH";
                userOrganisation.PIN = pin;
                userOrganisation.PINHash = Crypto.GetSHA512Checksum(pin);
                userOrganisation.PINSentDate = VirtualDateTime.Now;
                userOrganisation.PINConfirmedDate = VirtualDateTime.Now;
            }
            else
            {
                userOrganisation.PIN = pin;
                userOrganisation.PINHash = Crypto.GetSHA512Checksum(pin);
                userOrganisation.PINSentDate = pinSendDate.HasValue ? pinSendDate.Value : VirtualDateTime.Now;
                userOrganisation.PINConfirmedDate = null;
            }
            await dataRepository.SaveChangesAsync();

            return userOrganisation;
        }

        /// <summary>
        /// Returns an instance of the ISecurityCodeBusinessLogic domain interface
        /// </summary>
        /// <param name="host">The webhost</param>
        /// <returns></returns>
        public static ISecurityCodeBusinessLogic GetSecurityCodeBusinessLogic(this IHost host)
        {
            return host.Services.GetService<ISecurityCodeBusinessLogic>();
        }

        /// <summary>
        /// Returns an instance of the IOrganisationBusinessLogic domain interface
        /// </summary>
        /// <param name="host">The webhost</param>
        /// <returns></returns>
        public static IOrganisationBusinessLogic GetOrganisationBusinessLogic(this IHost host)
        {
            return host.Services.GetService<IOrganisationBusinessLogic>();
        }
    }
}
