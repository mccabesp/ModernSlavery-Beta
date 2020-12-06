using System;

namespace ModernSlavery.Core.Entities.StatementSummary.V1
{
    public class GovUkCountry : IComparable<GovUkCountry>
    {
        public string Name { get; }

        public string FullReference { get; }

        public string ShortReference { get; }

        public GovUkCountry(string name, string fullreference)
        {
            Name = name;
            FullReference = fullreference;
            ShortReference = FullReference.Substring(FullReference.IndexOf(":")).TrimStart(':');
        }

        public int CompareTo(GovUkCountry other)
        {
            return FullReference.CompareTo(other?.FullReference);
        }
    }
}
