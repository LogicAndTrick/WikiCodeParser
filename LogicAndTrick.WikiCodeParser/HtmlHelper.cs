using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace LogicAndTrick.WikiCodeParser
{
    internal static class HtmlHelper
    {
        public static string Encode(string text)
        {
            return HttpUtility.HtmlEncode(text);
        }

        public static string UrlEncode(string urlPart)
        {
            return HttpUtility.UrlEncode(urlPart);
        }

        public static string AttributeEncode(string attributeText)
        {
            return HttpUtility.HtmlAttributeEncode(attributeText)
                .Replace(">", "&gt;")
                .Replace("'", "&#39;");
        }
    }
}
