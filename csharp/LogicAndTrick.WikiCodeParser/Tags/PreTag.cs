﻿using System.Collections.Generic;
using System.Linq;
using LogicAndTrick.WikiCodeParser.Elements;
using LogicAndTrick.WikiCodeParser.Nodes;

namespace LogicAndTrick.WikiCodeParser.Tags
{
    public class PreTag : Tag
    {
        public PreTag()
        {
            Token = "pre";
            Element = "pre";
            IsBlock = true;
        }

        public override INode FormatResult(Parser parser, ParseData data, State state, string scope, Dictionary<string, string> options, string text)
        {
            var before = "<" + Element;
            if (ElementClass != null) before += " class=\"" + ElementClass + '"';
            before += "><code>";
            var after = "</code></" + Element + '>';

            var arr = text.Split('\n').ToList();

            // Trim blank lines from the start and end of the array
            for (var i = 0; i < 2; i++)
            {
                while (arr.Count > 0 && arr[0].Trim() == "") arr.RemoveAt(0);
                arr.Reverse();
            }

            arr = PreElement.FixCodeIndentation(arr);
            text = string.Join("\n", arr);

            return new HtmlNode(before, new UnprocessablePlainTextNode(text), after)
            {
                IsBlockNode = true
            };
        }
    }
}
