using System;
using System.Collections.Generic;
using System.Linq;
using ModernSlavery.Core;
using ModernSlavery.Core.Attributes;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Options;

namespace ModernSlavery.BusinessDomain.Account
{
    [Options("UserRoles")]
    public class UserRolesOptions: Dictionary<string, SortedSet<UserRoleTypes>>,IOptions
    {
        public UserRolesOptions():base(StringComparer.OrdinalIgnoreCase)
        {
        }


        private static readonly Dictionary<string, SortedSet<UserRoleTypes>> _emailRoles=new Dictionary<string, SortedSet<UserRoleTypes>>(StringComparer.OrdinalIgnoreCase);

        public bool HasRole(string emailAddress, params UserRoleTypes[] roleTypes)
        {
            if (!emailAddress.IsEmailAddress()) throw new ArgumentException($"Bad email address '{emailAddress}'", nameof(emailAddress));
            if (roleTypes==null || roleTypes.Length<1) throw new ArgumentNullException(nameof(roleTypes));

            if (!_emailRoles.ContainsKey(emailAddress))
            {
                _emailRoles[emailAddress] = new SortedSet<UserRoleTypes>(this.Where(kv => emailAddress.Like(kv.Key)).SelectMany(kv => kv.Value));
                if (_emailRoles[emailAddress].Contains(UserRoleTypes.DatabaseAdmin) || _emailRoles[emailAddress].Contains(UserRoleTypes.SuperAdmin) || _emailRoles[emailAddress].Contains(UserRoleTypes.DevOpsAdmin))
                    _emailRoles[emailAddress].Add(UserRoleTypes.BasicAdmin);
            }

            return _emailRoles[emailAddress].Intersect(roleTypes).Any();
        }
    }
}
