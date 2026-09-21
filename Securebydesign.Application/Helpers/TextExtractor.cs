using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
namespace Securebydesign.Application.Helpers
{
    public class TextExtractor
    {
        public string ExtractText(string xhtml)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(xhtml);

            return doc.DocumentNode.InnerText;
        }

        public string ReplaceContent(string xhtml, string oldValue, string newValue)
        {
            return xhtml.Replace(oldValue, newValue);
        }
    }
}
