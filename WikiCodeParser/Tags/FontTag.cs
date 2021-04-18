using System.Collections.Generic;
using WikiCodeParser.Nodes;

namespace WikiCodeParser.Tags
{
    public class FontTag : Tag
    {
        public FontTag()
        {
            Token = "font";
            Element = "span";
            MainOption = "color";
            Options = new[] {"color", "colour", "size"};
        }

        public override INode FormatResult(Parser parser, State state, string scope, Dictionary<string, string> options, string text)
        {
            var before = "<" + Element;
            if (ElementClass != null) before += " class=\"" + ElementClass + "\"";
            if (options.ContainsKey("color") || options.ContainsKey("colour") || options.ContainsKey("size"))
            {
                before += " style=\"";
                if (options.ContainsKey("color") && Colours.IsValidColor(options["color"])) before += "color: " + options["color"] + ";";
                else if (options.ContainsKey("colour") && Colours.IsValidColor(options["colour"])) before += "color: " + options["colour"] + ";";
                if (options.ContainsKey("size") && IsValidSize(options["size"])) before += "font-size: " + options["size"] + "px;";
                before += "\"";
            }
            before += ">";
            var content = parser.ParseTags(text, scope, IsBlock ? "block" : "inline");
            var after = "</" + Element + ">";
            return new HtmlNode(before, content, after);
        }
        
        internal static bool IsValidSize(string text)
        {
            return int.TryParse(text, out var num) && num >= 6 && num <= 40;
        }
    }
}
