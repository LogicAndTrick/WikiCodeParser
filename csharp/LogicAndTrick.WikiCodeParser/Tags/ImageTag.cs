﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using LogicAndTrick.WikiCodeParser.Nodes;

namespace LogicAndTrick.WikiCodeParser.Tags
{
    public class ImageTag : Tag
    {
        public ImageTag()
        {
            Token = "img";
            Element = "div";
            MainOption = "url";
            Options = new[] {"url"};
            IsBlock = true;
        }

        public override INode FormatResult(Parser parser, ParseData data, State state, string scope, Dictionary<string, string> options, string text)
        {
            var url = text;
            if (options.ContainsKey("url")) url = options["url"];
            if (!Regex.IsMatch(url, "^([a-z]{2,10}://)", RegexOptions.IgnoreCase)) url = "http://" + url;
            url = HtmlHelper.AttributeEncode(url);

            var classes = new List<string>{"embedded", "image"};
            if (ElementClass != null) classes.Add(ElementClass);
            var element = Element;

            if (!IsBlock)
            {
                element = "span";
                classes.Add("inline");
            }
            else
            {
                state.SkipWhitespace();
            }

            var before = $"<{element} class=\"{String.Join(" ", classes)}\">" +
                         $"<span class=\"caption-panel\">" +
                         $"<img class=\"caption-body\" src=\"{url}\" alt=\"User posted image\" />";
            var after = $"</span></{element}>";
            var plainsp = element == "div" ? "\n" : "";
            return new HtmlNode(before, PlainTextNode.Empty, after)
            {
                PlainBefore = $"{plainsp}[User posted image]{plainsp}",
                IsBlockNode = element == "div"
            };
        }

        public override bool Validate(Dictionary<string, string> options, string text)
        {
            var url = text;
            if (options.ContainsKey("url")) url = options["url"];
            return !url.Contains("<script") && Regex.IsMatch(url, @"^([a-z]{2,10}://)?([^]""\n ]+?)$", RegexOptions.IgnoreCase);
        }
    }
}