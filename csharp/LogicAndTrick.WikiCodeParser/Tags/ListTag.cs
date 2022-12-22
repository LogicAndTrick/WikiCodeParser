using System;
using System.Collections.Generic;
using System.Linq;
using LogicAndTrick.WikiCodeParser.Nodes;

namespace LogicAndTrick.WikiCodeParser.Tags
{
    public class ListTag : Tag
    {
        public ListTag()
        {
            Token = "list";
            Element = "ul";
            IsBlock = true;
        }

        public override bool Validate(Dictionary<string, string> options, string text)
        {
            var items = text.Split(new[] { "[*]" }, StringSplitOptions.None)
                .Select(x => x.Trim())
                .Where(x => x.Length > 0);
            return base.Validate(options, text) && items.Any();
        }

        public override INode FormatResult(Parser parser, ParseData data, State state, string scope, Dictionary<string, string> options, string text)
        {
            var before = "<" + Element;
            if (ElementClass != null) before += " class=\"" + ElementClass + "\"";
            before += ">\n";

            var content = new NodeCollection();
            var items = text.Split(new[] { "[*]" }, StringSplitOptions.None)
                .Select(x => x.Trim())
                .Where(x => x.Length > 0);
            foreach (var item in items)
            {
                content.Nodes.Add(new HtmlNode("<li>", parser.ParseTags(data, item, scope, TagContext), "</li>\n") { PlainBefore = "* ", PlainAfter = "\n" });
            }

            var after = "</" + Element + ">";
            return new HtmlNode(before, content, after) { IsBlockNode = true };
        }
    }
}
