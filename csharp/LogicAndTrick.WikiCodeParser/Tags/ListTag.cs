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
                .Select(x => Util.Trim(x))
                .Where(x => x.Length > 0);
            return base.Validate(options, text) && items.Any();
        }

        public override INode FormatResult(Parser parser, ParseData data, State state, string scope, Dictionary<string, string> options, string text, Position tagPosition, Position textPosition)
        {
            var before = "<" + Element;
            if (ElementClass != null) before += " class=\"" + ElementClass + "\"";
            before += ">\n";

            var content = new NodeCollection();

            var itemState = new State(textPosition.Line, textPosition.Column, text);
            var itemPos = textPosition;
            while (!itemState.Done)
            {
                var plainPos = itemState.SourcePosition;
                var plain = itemState.ScanTo("[*]");

                var itemNode = CreateListItemNode(parser, data, itemState, scope, plain, plainPos, itemPos);
                if (itemNode != null) content.Nodes.Add(itemNode);

                itemPos = new Position(itemState.SourceLine, itemState.SourceColumn);
                itemState.Seek(3, false); // read the [*]
            }

            var after = "</" + Element + ">";
            return new HtmlNode(tagPosition, before, content, after) { IsBlockNode = true };
        }

        private HtmlNode CreateListItemNode(Parser parser, ParseData data, State state, string scope, string text, Position textPosition, Position tagPosition)
        {
            text = Util.Trim(text);
            if (text.Length == 0) return null;
            
            return HtmlNodeBuilder.Make("li", tagPosition)
                .Add(parser.ParseTags(data, text, scope, TagContext, textPosition.Line, textPosition.Column))
                .ToNode(plainBefore: "* ", plainAfter: "\n", htmlAfter: "\n");
        }
    }
}
