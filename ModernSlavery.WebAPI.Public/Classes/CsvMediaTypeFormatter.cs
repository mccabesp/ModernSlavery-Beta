using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
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
                    if (val.Value != null)
                    {
                        var tmpval = val.Value.ToString();

                        //Check if the value contans a comma and place it in quotes if so
                        if (tmpval.Contains(","))
                            tmpval = string.Concat("\"", tmpval, "\"");

                        //Replace any \r or \n special characters from a new line with a space
                        tmpval = tmpval.Replace("\r", " ", StringComparison.InvariantCultureIgnoreCase);
                        tmpval = tmpval.Replace("\n", " ", StringComparison.InvariantCultureIgnoreCase);

                        values.Add(tmpval);
                    }
                    else
                    {
                        values.Add(string.Empty);
                    }
                }
                csv.AppendLine(string.Join(",", values));
            }
            return context.HttpContext.Response.WriteAsync(csv.ToString(), selectedEncoding);
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
