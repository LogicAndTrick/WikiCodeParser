using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using WikiCodeParser.Nodes;

namespace WikiCodeParser.Tags
{
    public class ImageTag : Tag
    {
        public override string Token => "img";
        public override string Element => "span";
        public override string MainOption => "url";
        public override string[] Options => new[] {"url"};

        public override INode FormatResult(Parser parser, State state, string scope, Dictionary<string, string> options, string text)
        {
            var url = text;
            if (options.ContainsKey("url")) url = options["url"];
            if (!Regex.IsMatch(url, "^([a-z]{2,10}://)", RegexOptions.IgnoreCase)) url = "http://" + url;
            url = System.Web.HttpUtility.HtmlAttributeEncode(url);

            var classes = new List<string>{"embedded", "image"};
            if (ElementClass != null) classes.Add(ElementClass);
            if (Token == "simg") classes.Add("inline");

            var element = Element;
            if (!classes.Contains("inline"))
            {
                state.SkipWhitespace();
                element = "div";
            }

            var before = $"<{element} class=\"{String.Join(" ", classes)}\">" +
                         $"<span class=\"caption-panel\">" +
                         $"<img class=\"caption-body\" src=\"{url}\" alt=\"User posted image\" />";
            var after = $"</span></{element}>";
            return new HtmlNode(before, PlainTextNode.Empty, after) { PlainBefore = "[User posted image]" };
        }

        public override bool Validate(Dictionary<string, string> options, string text)
        {
            var url = text;
            if (options.ContainsKey("url")) url = options["url"];
            return !url.Contains("<script") && Regex.IsMatch(url, @"^([a-z]{2,10}://)?([^]""\n ]+?)$", RegexOptions.IgnoreCase);
        }
    }
}