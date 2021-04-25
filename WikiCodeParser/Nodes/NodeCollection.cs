using System.Collections.Generic;
using System.Linq;

namespace WikiCodeParser.Nodes
{
    /// <summary>
    /// A list of nodes.
    /// </summary>
    public class NodeCollection : INode
    {
        public List<INode> Nodes { get; set; }

        public NodeCollection(params INode[] nodes) : this(nodes.AsEnumerable())
        {
            //
        }

        public NodeCollection(IEnumerable<INode> nodes)
        {
            Nodes = nodes.ToList();
        }

        public string ToHtml() => string.Join("", Nodes.Select(x => x.ToHtml()));
        public string ToPlainText() => string.Join("", Nodes.Select(x => x.ToPlainText()));
        public IList<INode> GetChildren() => Nodes;
        public void ReplaceChild(int i, INode node) => Nodes[i] = node;
    }
}