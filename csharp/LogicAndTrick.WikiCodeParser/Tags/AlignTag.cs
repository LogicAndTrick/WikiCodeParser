using LogicAndTrick.WikiCodeParser.Nodes;
using System;
using System.Collections.Generic;
using System.Text;

namespace LogicAndTrick.WikiCodeParser.Tags
{
    public class AlignTag : Tag
    {
        public AlignTag()
        {
            Token = "align";
            Element = "div";
            MainOption = "align";
            Options = new[] { "align" };
            AllOptionsInMain = true;
            IsBlock = true;
        }

        public override INode FormatResult(Parser parser, ParseData data, State state, string scope, Dictionary<string, string> options, string text)
        {
            var before = "<" + Element;
            var cls = ElementClass + " ";
            if (options.ContainsKey("align") && IsValidAlign(options["align"]))
            {
                cls += "text-" + ConvertAlign(options["align"]);
            }
            before += " class=\"" + cls.Trim() + "\">";
            var content = parser.ParseTags(data, text, scope, TagContext);
            var after = "</" + Element + ">";
            return new HtmlNode(before, content, after) { IsBlockNode = true };
        }

        private static bool IsValidAlign(string text)
        {
            return text == "left" || text == "right" || text == "center";
        }

        private static string ConvertAlign(string text)
        {
            if (text == "left") return "start";
            if (text == "right") return "end";
            return "center";
        }
    }
}
