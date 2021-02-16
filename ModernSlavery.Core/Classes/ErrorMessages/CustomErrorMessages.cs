using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ModernSlavery.Core.Extensions;

namespace ModernSlavery.Core.Classes.ErrorMessages
{
    public static class CustomErrorMessages
    {
        static CustomErrorMessages()
        {
            Load();
        }

        private static List<CustomErrorMessage> _customErrorMessages;

        public static CustomErrorMessage DefaultPageError
        {
            get { return _customErrorMessages.SingleOrDefault(e => e.Default==true); }
        }

        internal static void Load()
        {
            var filePath = $"{AppDomain.CurrentDomain.BaseDirectory}App_Data\\CustomErrorMessages.csv";

            filePath = FileSystem.ExpandLocalPath("App_Data\\CustomErrorMessages.csv");
            
            if (!File.Exists(filePath)) throw new FileNotFoundException("Cannot find CustomErrorMessages file", filePath);

            var content = File.ReadAllText(filePath);
            if (string.IsNullOrWhiteSpace(content)) throw new FileLoadException("No content in CustomErrorMessages file", filePath);

            _customErrorMessages =Extensions.ReadCSV<CustomErrorMessage>(content);

            if (!_customErrorMessages.Any()) throw new FileLoadException("No messages in CustomErrorMessages file", filePath);

            //Ensure all Validators are correct
            Parallel.ForEach(_customErrorMessages, customErrorMessage => customErrorMessage.SetValidator());

            var exceptions = new List<Exception>();
            //Check for duplicate defaults
            var defaults = _customErrorMessages.Where(e => e.Default == true);
            if (!defaults.Any())
                exceptions.Add(new ConfigurationException($"CustomErrorMessages must contains 1 record with {nameof(CustomErrorMessage.Default)}=true"));
            else
            {
                var duplicateDefaults = _customErrorMessages.Where(e => e.Default == true).GroupBy(e => e.Default).Where(g => g.Count() > 1).ToDictionary(x => x.Key, y => y.Count());
                foreach (var key in duplicateDefaults.Keys)
                    exceptions.Add(new ConfigurationException($"CustomErrorMessages contains {duplicateDefaults[key]} duplicates with {nameof(CustomErrorMessage.Default)}={key}"));
            }
            //Check for duplicate codes
            var duplicateCodes = _customErrorMessages.GroupBy(e => e.Code).Where(g => g.Count() > 1).ToDictionary(x => x.Key, y => y.Count());
            foreach (var key in duplicateCodes.Keys)
                exceptions.Add(new ConfigurationException($"CustomErrorMessages contains {duplicateCodes[key]} duplicates with {nameof(CustomErrorMessage.Code)}={key}"));

            //Check for duplicate validators
            var duplicateValidators = _customErrorMessages.Where(e => !string.IsNullOrWhiteSpace(e.Validator)).GroupBy(e => e.Validator, StringComparer.OrdinalIgnoreCase).Where(g => g.Count() > 1).ToDictionary(x => x.Key, y => y.Count());
            foreach (var key in duplicateValidators.Keys)
                exceptions.Add(new ConfigurationException($"CustomErrorMessages contains {duplicateValidators[key]} duplicates with {nameof(CustomErrorMessage.Validator)}={key}"));

            //Throw any exceptions
            if (exceptions.Count == 1) throw exceptions.First();
            if (exceptions.Count > 1) throw new AggregateException(exceptions);
        }

        public static CustomErrorMessage GetPageError(int code)
        {
            return _customErrorMessages.SingleOrDefault(e => e.Code == code);
        }

        public static CustomErrorMessage GetValidationError(string validator)
        {
            if (validator.Contains('*'))return _customErrorMessages.FirstOrDefault(e => e.Validator.Like(validator));

            return _customErrorMessages.SingleOrDefault(e => validator.Equals(e.Validator, StringComparison.OrdinalIgnoreCase));
        }

        public static string GetModelError(string validator)
        {
            return GetValidationError(validator)?.Description;
        }
    }

    [Serializable]
    public class CustomErrorMessage
    {
        public int Code { get; set; }

        public string Title { get; set; }

        public string Subtitle { get; set; }

        public string Description { get; set; }

        public string CallToAction { get; set; }

        public string ActionUrl { get; set; }

        public string Model { get; set; }
        public string Property { get; set; }
        public string Attribute { get; set; }
        public string Validator { get; private set; }

        public void SetValidator()
        {
            Validator = $"{Model}.{Property}:{Attribute}".Trim('.',':');
            Validator = string.IsNullOrWhiteSpace(Validator) ? null : Validator;
        }

        public string DisplayName { get; set; }

        public string ActionText { get; set; }

        public bool? Default { get; set; }

        public override string ToString()
        {
            return $"{Title} {Description}".Trim();
        }
    }
}