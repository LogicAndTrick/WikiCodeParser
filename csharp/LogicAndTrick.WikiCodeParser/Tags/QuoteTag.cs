using System.Collections.Generic;
using LogicAndTrick.WikiCodeParser.Nodes;

namespace LogicAndTrick.WikiCodeParser.Tags
{
    public class QuoteTag : Tag
    {
        public QuoteTag()
        {
            Token = "quote";
            Element = "blockquote";
            MainOption = "name";
            Options = new[] {"name"};
            AllOptionsInMain = true;
            IsBlock = true;
            IsNested = true;
        }

        public override INode FormatResult(Parser parser, ParseData data, State state, string scope, Dictionary<string, string> options, string text)
        {
            var before = "<" + Element;
            if (ElementClass != null) before += " class=\"" + ElementClass + '"';
            before += '>';
            if (options.ContainsKey("name"))
            {
                before += "<strong class=\"quote-name\">" + options["name"] + " said:</strong><br/>";
            }
            var after = "</" + Element + '>';
            var content = parser.ParseTags(data, text?.Trim(), scope, TagContext);
            return new HtmlNode(before, content, after)
            {
                PlainBefore = (options.ContainsKey("name") ? options["name"] + " said: " : "") + "[quote]\n",
                PlainAfter = "\n[/quote]",
                IsBlockNode = IsBlock
            };
        }
    }
}