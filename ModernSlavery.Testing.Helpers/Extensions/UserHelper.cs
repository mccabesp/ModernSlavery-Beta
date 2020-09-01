using EFCore.BulkExtensions;
using Microsoft.Extensions.Hosting;
using ModernSlavery.Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

/// <summary>
/// TODO: Use Business Logicfunctionality
/// </summary>

namespace ModernSlavery.Testing.Helpers.Extensions
{
    public static class UserHelper
    {
        public static User GetUser(this IHost host, string firstName, string lastName)
        {
            var dataRepository = host.GetDataRepository();
            return dataRepository.GetAll<User>().SingleOrDefault(u => u.Firstname == firstName && u.Lastname == lastName);
        }

        public static IEnumerable<User> ListUsers(this IHost host)
        {
            var dataRepository = host.GetDataRepository();
            return dataRepository.GetAll<User>();
        }

        public static User DeleteUser(this IHost host, string firstName, string lastName)
        {
            var dataRepository = host.GetDataRepository();
            var user = dataRepository.GetAll<User>().SingleOrDefault(u => u.Firstname == firstName && u.Lastname == lastName);
            var userId = user.UserId;
            var email = user.EmailAddress.ToLower();
            dataRepository.ExecuteTransactionAsync(async () =>
            {
                try
                {
                    dataRepository.GetAll<AuditLog>().Where(a=>a.OriginalUser!=null && a.OriginalUser.UserId==userId).BatchDelete();
                    dataRepository.GetAll<Feedback>().Where(f=>f.EmailAddress.ToLower()== email).BatchDelete();
                    dataRepository.GetAll<UserOrganisation>().Where(uo=>uo.UserId==userId).BatchDelete();
                    dataRepository.GetAll<ReminderEmail>().Where(r => r.UserId == userId).BatchDelete();
                    dataRepository.GetAll<User>().Where(u => u.UserId == userId).BatchDelete();
                    dataRepository.CommitTransaction();
                }
                catch
                {
                    dataRepository.RollbackTransaction();
                    throw;
                }
            }).Wait();
            return user;
        }

        //Gets the currently logged in user using the session Id
        public static User GetSignedInUser(this IHost host, string sessionId)
        {
            throw new NotImplementedException();
        }

        //Signs the user into the host and returns the session cookie
        public static string SignInUser(this IHost host, User user)
        {
            throw new NotImplementedException();
        }
        public static string ResetSignInAttempts(this User user, Organisation organisation)
        {
            throw new NotImplementedException();
        }

        public static string ResetSignUpAttempts(this User user, Organisation organisation)
        {
            throw new NotImplementedException();
        }

        public static string GetEmailVerifyUrl(this User user, Organisation organisation)
        {
            throw new NotImplementedException();
        }
        public static string ResetEmailVerifyAttempts(this User user, Organisation organisation)
        {
            throw new NotImplementedException();
        }

        public static string GetPinInPost(this User user, Organisation organisation)
        {
            throw new NotImplementedException();
        }
        public static string ResetPinInPostAttempts(this User user, Organisation organisation)
        {
            throw new NotImplementedException();
        }
        public static string ResetPasswordResetAttempts(this User user, Organisation organisation)
        {
            throw new NotImplementedException();
        }
    }
}
