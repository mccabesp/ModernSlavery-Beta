using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using ModernSlavery.Extensions;

namespace ModernSlavery.Core.Classes
{
    public interface ISourceComparer
    {
        bool CanReplace(string source, string target);
        bool CanReplace(string source, IEnumerable<string> targets);
        bool IsCoHo(string source);
        bool IsDnB(string source);
        int Parse(string source);
    }

    /// <summary>
    ///     Compares two data source types to see if one can replace the other
    /// </summary>
    public class SourceComparer: ISourceComparer
    {
        private readonly IConfiguration _configuration;
        public SourceComparer(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        private string AdminEmails => _configuration.GetValue<string>("AdminEmails");

        public bool CanReplace(string source, string target)
        {
            return Parse(source) >= Parse(target);
        }

        public bool CanReplace(string source, IEnumerable<string> targets)
        {
            foreach (string target in targets)
            {
                if (Parse(source) < Parse(target))
                {
                    return false;
                }
            }

            return true;
        }

        public int Parse(string source)
        {
            if (source.EqualsI("admin", "administrator") || IsAdministrator(source))
            {
                return 4;
            }

            if (IsCoHo(source))
            {
                return 3;
            }

            if (source.EqualsI("user") || source.IsEmailAddress())
            {
                return 2;
            }

            if (IsDnB(source) || source.EqualsI("Manual"))
            {
                return 1;
            }

            return 0;
        }

        public bool IsDnB(string source)
        {
            return source.Strip(" ").EqualsI("D&B", "DNB", "dunandbradstreet", "dun&bradstreet");
        }

        public bool IsCoHo(string source)
        {
            return source.Strip(" ").EqualsI("CoHo", "CompaniesHouse", "CompanyHouse");
        }

        public bool IsAdministrator(string emailAddress)
        {
            if (!emailAddress.IsEmailAddress()) return false;

            if (string.IsNullOrWhiteSpace(AdminEmails))
            {
                throw new ArgumentException("Missing AdminEmails from web.config");
            }

            return emailAddress.LikeAny(AdminEmails.SplitI(";"));
        }
    }

}
