using System;
using System.Collections.Generic;
using System.Text;

namespace ModernSlavery.Testing.Helpers
{
    public class Testing_Helpers
    {
        public static void ResetDB()
        {
            throw new NotImplementedException();
        }

        public static void CreateUser(string Firstname = null, string Lastname = null, string Title = null, string Email = null, string Password = null)
        {
            throw new NotImplementedException();
        }

        public static void RegisterOrganisation(string UserName, string OrgName, string OrgType = null)
        {
            throw new NotImplementedException();
        }

        public static void RegisterOrg_NotActivated(string UserName, string OrgName, string OrgType = null)
        {
            throw new NotImplementedException();
        }

        //add fastrack organisation to db for testing fastrack workflow
        //return org code
        public static string AddFastrackOrgToDB(string OrgnName, string Pin = null)
        {
            throw new NotImplementedException();
        }

        public static string AddExpiredSecurityCodeToOrg(string OrgName, string Pin = null)
        {
            throw new NotImplementedException();
        }


        public static string AddExpiredPinToOrg(string OrgName, string Pin = null)
        {
            throw new NotImplementedException();
        }
        

        public static void RegisterUsersToOrg(string[] UserNames, string OrgName)
        {
            throw new NotImplementedException();
        }

        public static void RemoveUser(string Username)
        {
            throw new NotImplementedException();
        }
        public static void RemoveOrg(string OrgName)
        {
            throw new NotImplementedException();
        }

        public static void RemoveAllDrafts()
        {
            throw new NotImplementedException();
        }

        public static void RemoveDraft(string OrgId, string Year)
        {
            throw new NotImplementedException();
        }
    }
}
