using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using LogicAndTrick.WikiCodeParser.Nodes;

namespace LogicAndTrick.WikiCodeParser.Processors
{
    public class NewLineProcessor : INodeProcessor
    {
        public int Priority { get; set; } = 1;

        public bool ShouldProcess(INode node, string scope)
        {
            return node is PlainTextNode ptn && (ptn.Text.Contains("\n") || ptn.Text.Contains("<br>"));
        }

        public IEnumerable<INode> Process(Parser parser, ParseData data, INode node, string scope)
        {
            var text = ((PlainTextNode) node).Text;
            text = Regex.Replace(text, " *<br> *", "\n");

            var lines = text.Split('\n');
            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                yield return new PlainTextNode(line);
                // Don't emit a line break after the final line of the text as it did not end with a newline
                if (i < lines.Length - 1) yield return new HtmlNode("<br/>", UnprocessablePlainTextNode.NewLine, "");
            }
        }
    }

    public class TrimWhitespaceAroundBlockNodesProcessor : INodeProcessor
    {
        public int Priority { get; set; } = 2;

        public bool ShouldProcess(INode node, string scope)
        {
            return node is NodeCollection;
        }

        public IEnumerable<INode> Process(Parser parser, ParseData data, INode node, string scope)
        {
            var coll = (NodeCollection) node;

            var trimStart = false;
            for (var i = 0; i < coll.Nodes.Count; i++)
            {
                var child = coll.Nodes[i];
                var next = i < coll.Nodes.Count - 1 ? coll.Nodes[i + 1] : null;
                if (child is PlainTextNode ptn)
                {
                    var text = ptn.Text;
                    if (trimStart) text = text.TrimStart();
                    if (next is HtmlNode nht && nht.IsBlockNode) text = text.TrimEnd();
                    ptn.Text = text;
                }

                child = parser.RunProcessor(child, this, data, scope);

                if (child is HtmlNode html && html.IsBlockNode)
                {
                    trimStart = true;
                    yield return UnprocessablePlainTextNode.NewLine;
                    yield return child;
                    yield return UnprocessablePlainTextNode.NewLine;
                }
                else
                {
                    trimStart = false;
                    yield return child;
                }
            }
        }
    }
}
