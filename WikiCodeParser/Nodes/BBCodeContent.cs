using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WikiCodeParser.Nodes
{
    public class BBCodeContent
    {
        public string HtmlBefore { get; set; }
        public string HtmlAfter { get; set; }
        public string PlainTextContent { get; set; }
        public List<BBCodeContent> Children { get; set; }
        public Dictionary<string, List<string>> Meta { get; set; }

        public BBCodeContent()
        {
            Children = new List<BBCodeContent>();
            Meta = new Dictionary<string, List<string>>();
            HtmlBefore = HtmlAfter = PlainTextContent = null;
        }

        public BBCodeContent(string before, string after, BBCodeContent child) : this()
        {
            if (child != null) Children.Add(child);
            HtmlBefore = before;
            HtmlAfter = after;
        }

        public BBCodeContent(string before, string after, string text) : this()
        {
            HtmlBefore = before;
            HtmlAfter = after;
            PlainTextContent = text;
        }

        public void AddMeta(string key, string val)
        {
            if (!Meta.ContainsKey(key)) Meta[key] = new List<string> {val};
            else Meta[key].Add(val);
        }

        public Dictionary<string, List<string>> GetMeta()
        {
            var meta = new Dictionary<string, List<string>>(Meta);
            foreach (var c in Children)
            {
                foreach (var m in c.GetMeta())
                {
                    if (!meta.ContainsKey(m.Key)) meta.Add(m.Key, m.Value);
                    else meta[m.Key].AddRange(m.Value);
                }
            }

            return meta;
        }

        public string ToHtml()
        {
            var sb = new StringBuilder();
            if (HtmlBefore != null) sb.Append(HtmlBefore);
            if (Children.Any())
            {
                if (PlainTextContent != null)
                    throw new InvalidOperationException("BBCodeContent cannot have both children and text content.");
                foreach (var c in Children) sb.Append(c.ToHtml());
            }
            else
            {
                if (PlainTextContent != null) sb.Append(PlainTextContent);
            }

            if (HtmlAfter != null) sb.Append(HtmlAfter);
            return sb.ToString();
        }

        public string ToPlainText()
        {
            if (Children.Any())
            {
                if (PlainTextContent != null)
                    throw new InvalidOperationException("BBCodeContent cannot have both children and text content.");
                var sb = new StringBuilder();
                foreach (var c in Children) sb.Append(c.ToPlainText());
                return sb.ToString();
            }

            return PlainTextContent;
        }

        public static string HtmlEncode(string text)
        {
            return System.Web.HttpUtility.HtmlEncode(text);
        }

        public static string UrlEncode(string text)
        {
            return System.Web.HttpUtility.UrlEncode(text);
        }
    }
}