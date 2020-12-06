using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using ModernSlavery.Core.Classes.StatementTypeIndexes;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Models;
using ModernSlavery.WebAPI.Models;
using ModernSlavery.WebUI.Shared.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ModernSlavery.Core.Entities.StatementSummary.V1.StatementSummary;
using static ModernSlavery.Core.Entities.StatementSummary.V1.StatementSummary.StatementRisk;

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
        public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            Type type = GetTypeOf(context.Object);
            if (type == typeof(StatementSummaryDownloadModel))
                await WriteResponseBodyStatementSummaryAsync(context, selectedEncoding);
            else
                await WriteResponseBodyGenericAsync(context, selectedEncoding);
        }

        private async Task WriteResponseBodyStatementSummaryAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            StringBuilder csv = new StringBuilder();

            var properties = typeof(StatementSummaryDownloadModel).GetProperties().ToList();

            csv.AppendLine(string.Join(",", properties.Select(x => x.Name)));

            foreach (var item in (IEnumerable<StatementSummaryDownloadModel>)context.Object)
            {
                var values = properties.Select(p => AddQuotes(ConvertToText(p.GetValue(item, null)) ?? string.Empty));

                csv.AppendLine(string.Join(",", values));
            }
            await WriteText(context, csv.ToString(), selectedEncoding,true);
        }

        private async Task WriteResponseBodyGenericAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
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

            await WriteText(context, csv.ToString(), selectedEncoding, true);
        }

        private async Task WriteText(OutputFormatterWriteContext context, string text, Encoding selectedEncoding, bool includeByteOrderMark=false)
        {
            //Write the BOM
            if (includeByteOrderMark)
            {
                var preamble = selectedEncoding.GetPreamble();
                await context.HttpContext.Response.Body.WriteAsync(preamble, 0, preamble.Length);
            }

            //Write the text
            await context.HttpContext.Response.WriteAsync(text, selectedEncoding);
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
                case bool boolValue:
                    return boolValue ? "Yes" : "No";
                case DateTime dateValue:
                    return dateValue.ToShortDateString();
                case AddressModel addressModel:
                    return addressModel.GetFullAddress(Environment.NewLine);
                case SectorTypeIndex.SectorType sectorType:
                    return sectorType.Description;
                case PolicyTypes type:
                    return type.GetEnumDescription();
                case TrainingTargetTypes type:
                    return type.GetEnumDescription();
                case PartnerTypes type:
                    return type.GetEnumDescription();
                case SocialAuditTypes type:
                    return type.GetEnumDescription();
                case GrievanceMechanismTypes type:
                    return type.GetEnumDescription();
                case RiskSourceTypes type:
                    return type.GetEnumDescription();
                case RiskTargetTypes type:
                    return type.GetEnumDescription();
                case IndicatorTypes type:
                    return type.GetEnumDescription();
                case RemediationTypes type:
                    return type.GetEnumDescription();
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
            return typeof(IEnumerable).IsAssignableFrom(type) || type.IsAsyncEnumerable();
        }
    }
}
