using System.Collections.Generic;
using LogicAndTrick.WikiCodeParser.Nodes;

namespace LogicAndTrick.WikiCodeParser.Tags
{
    public class SizeTag : Tag
    {
        public SizeTag()
        {
            Token = "size";
            Element = "span";
            MainOption = "size";
            Options = new[] { "size" };
            AllOptionsInMain = true;
        }

        public override INode FormatResult(Parser parser, ParseData data, State state, string scope, Dictionary<string, string> options, string text)
        {
            var before = "<" + Element;
            if (ElementClass != null) before += " class=\"" + ElementClass + "\"";
            if (options.ContainsKey("size"))
            {
                before += " style=\"";
                if (options.ContainsKey("size") && FontTag.IsValidSize(options["size"])) before += "font-size: " + options["size"] + "px; ";
                before = before.TrimEnd(' ') + "\"";
            }
            before += ">";
            var content = parser.ParseTags(data, text, scope, TagContext);
            var after = "</" + Element + ">";
            return new HtmlNode(before, content, after);
        }
    }
}