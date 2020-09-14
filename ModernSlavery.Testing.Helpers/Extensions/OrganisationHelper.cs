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
        /// <param name="email">The email of the user</param>
        /// <param name="pin">If empty the registration is activated. Otherwise unactivated and pin set to this value</param>
        /// <param name="pinSendDate">When pin not null this value marks when the pin was sent - set it to an old value to expire the pin</param>
        /// <returns></returns>
        public static async Task<UserOrganisation> RegisterUserOrganisationAsync(this IHost host, string organisationName, string email, string pin = null, DateTime? pinSendDate = null)
        {
            // guards against misuse
            if (host == null)
                throw new ArgumentNullException(nameof(host));
            if (string.IsNullOrEmpty(organisationName))
                throw new ArgumentNullException(nameof(organisationName), "Organisation name can not be null or empty");
            if (string.IsNullOrEmpty(email))
                throw new ArgumentNullException(nameof(email), "Email can not be null or empty");

            var dataRepository = host.GetDataRepository();
            Organisation organisation = dataRepository.GetAll<Organisation>().SingleOrDefault(o => o.OrganisationName == organisationName);
            if (organisation == null)
                throw new ArgumentException(nameof(organisationName), "No corresponding organisation matches the supplied organisation name");
            var organisationId = organisation.OrganisationId;

            var user = dataRepository.GetAll<User>().SingleOrDefault(u => u.EmailAddress == email);
            if (user == null)
                throw new ArgumentException(nameof(email), "No corresponding user matches the supplied email");
            var userId = user.UserId;

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
        /// Shim for saving when needed, should prob refactor this out.
        /// </summary>
        /// <returns></returns>
        public static async Task SaveAsync(this IHost host)
        {
            await host.Services.GetService<Core.Interfaces.IDataRepository>().SaveChangesAsync();
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

        public static IScopeBusinessLogic GetScopeBusinessLogic(this IHost host)
            => host.Services.GetService<IScopeBusinessLogic>();
    }
}
