using Microsoft.AspNetCore.Mvc.Formatters;
using ModernSlavery.Core.Extensions;
using ModernSlavery.WebAPI.Models;
using ModernSlavery.WebUI.Shared.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace ModernSlavery.WebAPI.Public.Classes
{
    public class StatementSummaryXmlFormatter : XmlSerializerOutputFormatter
    {
        protected override void Serialize(XmlSerializer xmlSerializer, XmlWriter xmlWriter, object value)
        {
            xmlSerializer = new XmlSerializer(value.GetType(), new XmlRootAttribute("StatementSummaries"));
            xmlSerializer.Serialize(xmlWriter, value);
        }

        protected override bool CanWriteType(Type type)
        {
            var canWriteType = type == typeof(IEnumerable<StatementSummaryDownloadModel>)
                || type == typeof(List<StatementSummaryDownloadModel>)
                || type == typeof(IAsyncEnumerable<StatementSummaryDownloadModel>);
            return canWriteType;
        }
    }
}
