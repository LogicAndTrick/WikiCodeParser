using System;
using System.Collections.Generic;
using System.Text;

namespace LogicAndTrick.WikiCodeParser.Nodes
{
    internal class RemovedNode : INode
    {
        public INode OriginalNode { get; set; }

        public RemovedNode(INode originalNode)
        {
            OriginalNode = originalNode;
        }

        public string ToHtml() => "";
        public string ToPlainText() => "";
        public IList<INode> GetChildren() => new List<INode>();
        public void ReplaceChild(int i, INode node) => throw new NotImplementedException();
        public bool HasContent() => false;
    }
}
