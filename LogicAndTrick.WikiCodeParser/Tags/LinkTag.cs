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
            Options = new[] {"url"};
        }

        public override INode FormatResult(Parser parser, ParseData data, State state, string scope, Dictionary<string, string> options, string text)
        {
            var url = text;
            if (options.ContainsKey("url")) url = options["url"];
            if (Token == "email") url = "mailto:" + url;
            else if (!Regex.IsMatch(url, "^([a-z]{2,10}://)", RegexOptions.IgnoreCase)) url = "http://" + url;
            url = HtmlHelper.AttributeEncode(url);

            var classes = new List<string>();
            if (ElementClass != null) classes.Add(ElementClass);

            var before = $"<{Element} " + (classes.Any() ? $"class=\"{String.Join(" ", classes)}\" " : "") + $"href=\"{url}\">";
            var after = $"</{Element}>";

            var content = options.ContainsKey("url")
                ? parser.ParseTags(data, text, scope, IsBlock ? "block" : "inline")
                : new UnprocessablePlainTextNode(text);
            return new HtmlNode(before, content, after);
        }

        public override bool Validate(Dictionary<string, string> options, string text)
        {
            var url = text;
            if (options.ContainsKey("url")) url = options["url"];
            return !url.Contains("<script") && Regex.IsMatch(url, @"^([a-z]{2,10}://)?([^]""\n ]+?)$", RegexOptions.IgnoreCase);
        }
    }
}