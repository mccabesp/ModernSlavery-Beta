using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using ModernSlavery.Core.Classes.StatementTypeIndexes;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModernSlavery.WebAPI.Public.Classes
{
    /// <summary>
    /// Formatter to generate csv
    /// </summary>
    public class CsvMediaTypeFormatter : TextOutputFormatter
    {

        /// <summary>
        /// CSV Formatter
        /// </summary>
        public CsvMediaTypeFormatter()
        {
            SupportedMediaTypes.Add(Microsoft.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/csv"));
            SupportedMediaTypes.Add(Microsoft.Net.Http.Headers.MediaTypeHeaderValue.Parse("text/csv"));
            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(Encoding.Unicode);
        }

        /// <summary>
        /// Write the response
        /// </summary>
        /// <param name="context"></param>
        /// <param name="selectedEncoding"></param>
        /// <returns></returns>
        public override Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            StringBuilder csv = new StringBuilder();
            Type type = GetTypeOf(context.Object);

            csv.AppendLine(
                string.Join<string>(
                    ",", type.GetProperties().Select(x => x.Name)
                )
            );

            foreach (var obj in (IEnumerable<object>)context.Object)
            {
                var vals = obj.GetType().GetProperties().Select(
                    pi => new
                    {
                        Value = pi.GetValue(obj, null)
                    }
                );

                List<string> values = new List<string>();
                foreach (var val in vals)
                {
                    string text;
                    if (val.Value is string)
                        text = val.Value.ToString();
                    else if (val.Value is IEnumerable list)
                        text = string.Join(Environment.NewLine, ConvertToText(list));
                    else
                        text = ConvertToText(val.Value);

                    values.Add(AddQuotes(text) ?? string.Empty);
                }
                csv.AppendLine(string.Join(",", values));
            }
            return context.HttpContext.Response.WriteAsync(csv.ToString(), selectedEncoding);
        }

        private static IEnumerable<string> ConvertToText(IEnumerable list)
        {
            foreach (var value in list)
            {
                var text = ConvertToText(value);
                if (text!=null) yield return text;
            }
        }

        private static string ConvertToText(object value)
        {
            if (value == null) return null;

            switch (value)
            {
                case string text:
                    return text;
                case AddressModel addressModel:
                    return addressModel.GetFullAddress(Environment.NewLine);
                case SectorTypeIndex.SectorType sectorType:
                    return sectorType.Description;
                case PolicyTypeIndex.PolicyType policyType:
                    return policyType.Description;
                case RiskTypeIndex.RiskType riskType:
                    return riskType.Description;
                case DiligenceTypeIndex.DiligenceType diligenceType:
                    return diligenceType.Description;
                case TrainingTypeIndex.TrainingType trainingType:
                    return trainingType.Description;
                default:
                    return value.ToString();
            }
        }

        private static string AddQuotes(string text)
        {
            if (text == null) return null;

            //Check if the value contans a comma and place it in quotes if so
            if (text.Contains(",") || text.Contains(Environment.NewLine))
                text = string.Concat("\"", text, "\"");

            return text;
        }

        private static Type GetTypeOf(object obj)
        {
            Type type = obj.GetType();
            Type itemType;
            if (type.GetGenericArguments().Length > 0)
            {
                itemType = type.GetGenericArguments()[0];
            }
            else
            {
                itemType = type.GetElementType();
            }
            return itemType;
        }

        protected override bool CanWriteType(Type type)
        {
            return typeof(IEnumerable).IsAssignableFrom(type);
        }
    }
}
