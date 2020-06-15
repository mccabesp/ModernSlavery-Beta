using AngleSharp.Common;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.XPath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModernSlavery.Testing.Helpers.Extensions
{
    public static class HtmlDocument
    {
        private static IEnumerable<INode> GetNodes(this IElement element, string selector)
        {
            if (string.IsNullOrWhiteSpace(selector))
            {
                throw new ArgumentNullException(nameof(selector));
            }

            try
            {
                List<INode> targets = element.SelectNodes(selector);
                if (targets.Any())return targets;

                return element.QuerySelectorAll(selector).Cast<INode>().ToList();
            }
            catch (DomException ex)
            {
                return null;
            }
        }

        public static INode GetHtmlNode(this IElement element, string selector)
        {
            return element.GetNodes(selector)?.FirstOrDefault();
        }

        public static IEnumerable<INode> GetHtmlNodes(this IElement element, string selector)
        {
            return element.GetNodes(selector);
        }

        public static INode GetHtmlNode(this IHtmlDocument responseDocument, string selector)
        {
            return responseDocument.DocumentElement.GetHtmlNode(selector);
        }

        public static IEnumerable<INode> GetHtmlNodes(this IHtmlDocument responseDocument, string selector)
        {
            return responseDocument.DocumentElement.GetHtmlNodes(selector);
        }

        public static string GetHtmlElementValue(this IHtmlDocument responseDocument, string selector, string attributeName = null)
        {
            if (string.IsNullOrWhiteSpace(selector))
            {
                throw new ArgumentNullException(nameof(selector));
            }

            INode target = responseDocument.GetHtmlNode(selector);
            if (target != null)
            {
                var element = target as IHtmlElement;
                if (element != null && !string.IsNullOrWhiteSpace(attributeName))
                {
                    IAttr attribute = element.Attributes[attributeName];
                    if (attribute == null)
                    {
                        return null;
                    }

                    return attribute.Value;
                }

                return target.Text();
            }

            return null;
        }
        public static IEnumerable<IHtmlMetaElement> GetMetaTags(this IHtmlDocument document)
        {
            return document.GetElementsByTagName("meta").Cast<IHtmlMetaElement>();
        }
        public static IEnumerable<IHtmlAnchorElement> GetAnchors(this IHtmlDocument document)
        {
            return document.GetElementsByTagName("a").Cast<IHtmlAnchorElement>();
        }
        public static IEnumerable<IHtmlLabelElement> GetLabels(this IHtmlDocument document)
        {
            return document.GetElementsByTagName("label").Cast<IHtmlLabelElement>();
        }

        public static IEnumerable<IHtmlHeadingElement> GetHeadings(this IHtmlDocument document)
        {
            return document.GetElementsByTagName("h1")
                .Concat(document.GetElementsByTagName("h2"))
                .Concat(document.GetElementsByTagName("h3"))
                .Concat(document.GetElementsByTagName("h4"))
                .Concat(document.GetElementsByTagName("h5"))
                .Concat(document.GetElementsByTagName("h6")).Cast<IHtmlHeadingElement>();
        }

        public static IEnumerable<XmlElement> GetFormFields(this IHtmlDocument content)
        {
            var fields = new SortedDictionary<string, string>();
            foreach (IHtmlFormElement form in content.Forms)
            {
                foreach (IHtmlElement element in form.Elements.OfType<IHtmlInputElement>())
                {
                    yield return element.ToXmlElement();
                }
            }
        }
    }
}
