using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace LogicAndTrick.WikiCodeParser
{
    internal static class HtmlHelper
    {
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
        
        private static readonly string[] AllowedUrlSchemes = { "http", "https", "mailto", "ftp" };

        public static bool ValidateUrl(string url)
        {
            if (url == null) return false;
            var scheme = GetUrlScheme(StripControlCharacters(url).Trim());
            return scheme == null || Array.IndexOf(AllowedUrlSchemes, scheme) >= 0;
        }

        /// <summary>
        /// Sanitises a url such that it always has one of our allowed url schemes (http, https, mailto, ftp).
        /// Actual validity of the url is not a consideration for this method - if the URL has no valid scheme,
        /// then https:// is added to the front by default.
        /// </summary>
        public static string SanitiseUrl(string url)
        {
            if (url == null) return "";
            var scheme = GetUrlScheme(StripControlCharacters(url).Trim());
            var hasValidScheme = Array.IndexOf(AllowedUrlSchemes, scheme) >= 0;
            return hasValidScheme ? url.Trim() : "https://" + url.Trim();
        }

        private static readonly HashSet<string> AllowedTagNames = new HashSet<string>
        {
            "div", "span",
            "h1", "h2", "h3", "h4", "h5", "h6",
            "a", "hr", "br",
            "img", "video", "audio",
            "pre", "code",
            "ul", "ol", "li",
            "blockquote",
            "strong", "em",
            "table", "thead", "tbody", "tfoot", "tr", "th", "td",
        };

        private static readonly HashSet<string> AllowedAttributeNames = new HashSet<string>
        {
            "id", "class", "style",
            "href", "src",
            "alt", "title",
            "playsinline", "controls", "autoplay", "loop", "muted"
        };

        private static readonly HashSet<string> AllowedStylePropertyNames = new HashSet<string>
        {
            "font-size", "color",
        };

        private static readonly Regex UnsafeStyleValueRegex = new Regex("[^#a-zA-Z0-9.]", RegexOptions.Compiled);

        public static bool ValidateTagName(string name)
        {
            if (name == null) return false;
            return AllowedTagNames.Contains(name);
        }

        public static bool ValidateAttributeName(string name)
        {
            if (name == null) return false;
            if (name.Any(c => c != '-' && (c < '0' || c > '9') && (c < 'a' || c > 'z'))) return false;
            if (name.StartsWith("data-")) return true;
            return AllowedAttributeNames.Contains(name);
        }

        public static bool ValidateStylePropertyName(string name)
        {
            return name != null && AllowedStylePropertyNames.Contains(name);
        }

        public static bool ValidateStyleValue(string value)
        {
            return value != null && !UnsafeStyleValueRegex.IsMatch(value);
        }
    }
}
