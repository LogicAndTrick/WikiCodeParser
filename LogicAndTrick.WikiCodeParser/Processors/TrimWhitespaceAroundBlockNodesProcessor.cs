using System.Collections.Generic;
using LogicAndTrick.WikiCodeParser.Nodes;

namespace LogicAndTrick.WikiCodeParser.Processors
{
    public class TrimWhitespaceAroundBlockNodesProcessor : INodeProcessor
    {
        public int Priority { get; set; } = 20;

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