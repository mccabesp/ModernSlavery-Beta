using Microsoft.Extensions.Hosting;
using ModernSlavery.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// TODO: Use Business Logicfunctionality
/// </summary>

namespace ModernSlavery.Testing.Helpers.Extensions
{
    public static class UserHelper
    {
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
