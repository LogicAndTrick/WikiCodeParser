using System.Collections.Generic;
using WikiCodeParser.Nodes;

namespace WikiCodeParser.Tags
{
    public class QuoteTag : Tag
    {
        public override string Token => "quote";
        public override string Element => "blockquote";
        public override string MainOption => "name";
        public override string[] Options => new[] {"name"};
        public override bool AllOptionsInMain => true;
        public override bool IsBlock => true;
        public override bool IsNested => true;

        public override INode FormatResult(Parser parser, State state, string scope, Dictionary<string, string> options, string text)
        {
            var before = "<" + Element;
            if (ElementClass != null) before += " class=\"" + ElementClass + '"';
            before += '>';
            if (options.ContainsKey("name"))
            {
                before += "<strong class=\"quote-name\">" + options["name"] + " said:</strong>";
            }
            var after = "</" + Element + '>';
            var content = parser.ParseTags(text, scope, IsBlock ? "block" : "inline");
            return new HtmlNode(before, content, after)
            {
                PlainBefore = options.ContainsKey("name") ? options["name"] + " said: " : ""
            };
        }
    }
}