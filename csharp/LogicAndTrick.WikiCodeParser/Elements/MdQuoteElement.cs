using System;
using System.Collections.Generic;
using LogicAndTrick.WikiCodeParser.Nodes;

namespace LogicAndTrick.WikiCodeParser.Elements
{
    public class MdQuoteElement : Element
    {
        public override bool Matches(Lines lines)
        {
            var value = lines.Value();
            return value.Length > 0 && value[0] == '>';
        }

        public override INode Consume(Parser parser, ParseData data, Lines lines, string scope)
        {
            var value = lines.Value();
            var arr = new List<string>
            {
                value.Substring(1).Trim()
            };
            while (lines.Next())
            {
                value = lines.Value().Trim();
                if (value.Length == 0 || value[0] != '>')
                {
                    lines.Back();
                    break;
                }
                arr.Add(value.Substring(1).Trim());
            }

            var text = String.Join("\n", arr);
            return new HtmlNode("<blockquote>", parser.ParseElements(data, text, scope), "</blockquote>");
        }
    }
}
