using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace LogicAndTrick.WikiCodeParser
{
    internal static class HtmlHelper
    {
        private static readonly string[] AllowedUrlSchemes = { "http", "https", "mailto", "ftp" };

        public static string Encode(string text)
        {
            text = text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;").Replace("'", "&#39;");
            return text;
        }

        public static string UrlEncode(string urlPart)
        {
            return HttpUtility.UrlEncode(urlPart);
        }

        public static string AttributeEncode(string attributeText)
        {
            return Encode(attributeText);
        }

        public static string StripControlCharacters(string text)
        {
            return text == null ? text : Regex.Replace(text, "[\\x00-\\x1F\\x7F]", "");
        }

        public static string GetUrlScheme(string url)
        {
            if (url == null) return null;
            var match = Regex.Match(url, "^([a-zA-Z][a-zA-Z0-9+.-]*):");
            return match.Success ? match.Groups[1].Value.ToLowerInvariant() : null;
        }

        public static bool ValidateUrl(string url)
        {
            if (url == null) return false;
            var scheme = GetUrlScheme(StripControlCharacters(url));
            return scheme == null || Array.IndexOf(AllowedUrlSchemes, scheme) >= 0;
        }
    }
}
