using Microsoft.AspNetCore.Mvc.Formatters;
using ModernSlavery.WebUI.Shared.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace ModernSlavery.WebAPI.Public.Classes
{
    public class XmlMediaTypeFormatter : XmlSerializerOutputFormatter
    {
        protected override void Serialize(XmlSerializer xmlSerializer, XmlWriter xmlWriter, object value)
        {
            if (value is List<StatementSummaryViewModel>)
                xmlSerializer = new XmlSerializer(typeof(List<StatementSummaryViewModel>), new XmlRootAttribute("StatementSummaries"));

            xmlSerializer.Serialize(xmlWriter, value);
        }
    }
}
