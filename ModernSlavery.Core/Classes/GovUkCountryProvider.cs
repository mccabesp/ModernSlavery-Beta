using ModernSlavery.Core.Entities.StatementSummary.V1;
using ModernSlavery.Core.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ModernSlavery.Core.Classes
{
    public class GovUkCountryProvider : IGovUkCountryProvider
    {
        readonly string FILE_PATH = $"{AppDomain.CurrentDomain.BaseDirectory}App_Data\\location-autocomplete-canonical-list.json";
        private Lazy<List<GovUkCountry>> Countries { get; }

        public GovUkCountryProvider()
        {
            Countries = new Lazy<List<GovUkCountry>>(() => ReadCountriesFile());
        }

        private List<GovUkCountry> ReadCountriesFile()
        {
            var countries = new List<GovUkCountry>();
            using (StreamReader file = File.OpenText(FILE_PATH))
            {
                JsonSerializer serializer = new JsonSerializer();
                var result = (List<List<string>>)serializer.Deserialize(file, typeof(List<List<string>>));
                countries.AddRange(result.Select(r => new GovUkCountry(r.First(), r.Last())));
            }
            return countries;
        }

        public IEnumerable<GovUkCountry> GetAll()
            => Countries.Value.AsEnumerable();

        public GovUkCountry FindByReference(string reference)
        {
            return GetAll().SingleOrDefault(c => c.FullReference == reference);
        }

        public GovUkCountry FindByName(string name)
        {
            return GetAll().SingleOrDefault(c => c.Name == name);
        }

        public GovUkCountry Find(string text)
        {
            var referenceResult = FindByReference(text);
            if (referenceResult != null)
                return referenceResult;

            var nameResult = FindByName(text);
            if (nameResult != null)
                return nameResult;

            return null;
        }
    }
}
