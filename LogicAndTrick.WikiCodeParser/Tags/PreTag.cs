﻿using System.Collections.Generic;
using LogicAndTrick.WikiCodeParser.Nodes;

namespace LogicAndTrick.WikiCodeParser.Tags
{
    public class PreTag : Tag
    {
        public PreTag()
        {
            Token = "pre";
            Element = "pre";
        }

        public override INode FormatResult(Parser parser, ParseData data, State state, string scope, Dictionary<string, string> options, string text)
        {
            var before = "<" + Element;
            if (ElementClass != null) before += " class=\"" + ElementClass + '"';
            before += "><code>";
            var after = "</code></" + Element + '>';
            return new HtmlNode(before, new UnprocessablePlainTextNode(text.Trim()), after)
            {
                IsBlockNode = true
            };
        }
    }
}
