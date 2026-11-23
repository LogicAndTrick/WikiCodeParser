using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using LogicAndTrick.WikiCodeParser.Nodes;

namespace LogicAndTrick.WikiCodeParser.Tags
{
    public class LinkTag : Tag
    {
        public LinkTag()
        {
            Token = "url";
            Element = "a";
            MainOption = "url";
            Options = new[] { "url" };
        }

        public override INode FormatResult(Parser parser, ParseData data, State state, string scope, Dictionary<string, string> options, string text, Position tagPosition, Position textPosition)
        {
            var url = BuildUrl(options, text);
            url = HtmlHelper.AttributeEncode(url);

            var classes = new List<string>();
            if (ElementClass != null) classes.Add(ElementClass);

            var before = $"<{Element} " + (classes.Any() ? $"class=\"{String.Join(" ", classes)}\" " : "") + $"href=\"{url}\">";
            var after = $"</{Element}>";

            var content = options.ContainsKey("url")
                ? parser.ParseTags(data, text, scope, TagContext, textPosition.Line, textPosition.Column)
                : new UnprocessablePlainTextNode(textPosition, text);
            return new HtmlNode(tagPosition, before, content, after);
        }

        public override bool Validate(Dictionary<string, string> options, string text)
        {
            if (options.ContainsKey("url") && Util.Trim(options["url"] ?? "").Length == 0) return false;
            var url = BuildUrl(options, text);
            return HtmlHelper.ValidateUrl(url) && Regex.IsMatch(url, @"^[^\]""\n ]+$", RegexOptions.IgnoreCase);
        }

        private string BuildUrl(Dictionary<string, string> options, string text)
        {
            var url = text;
            if (options.ContainsKey("url")) url = options["url"];
            if (!HtmlHelper.ValidateUrl(url)) return "";
            if (Token == "email") url = "mailto:" + Util.Trim(url);
            return HtmlHelper.SanitiseUrl(url);
        }
    }
}