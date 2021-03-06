using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using WikiCodeParser.Nodes;

namespace WikiCodeParser.Tags
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
            url = System.Web.HttpUtility.HtmlAttributeEncode(url);

            var classes = new List<string>();
            if (ElementClass != null) classes.Add(ElementClass);

            var before = $"<{Element} class=\"{String.Join(" ", classes)}\" href=\"{url}\">";
            var after = $"</{Element}>";

            var content = options.ContainsKey("url")
                ? parser.ParseTags(data, text, scope, IsBlock ? "block" : "inline")
                : new PlainTextNode(text);
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