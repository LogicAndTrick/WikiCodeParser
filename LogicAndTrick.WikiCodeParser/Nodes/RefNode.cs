using System;
using System.Collections.Generic;

namespace LogicAndTrick.WikiCodeParser.Nodes
{
    public class RefNode : INode
    {
        public ParseData Data { get; set; }
        public string Name { get; set; }

        public RefNode(ParseData data, string name)
        {
            Data = data;
            Name = name;
        }

        private INode GetNode()
        {
            return Data.Get($"Ref::{Name}", () => UnprocessablePlainTextNode.Empty);
        }

        public string ToHtml()
        {
            return GetNode().ToHtml();
        }

        public string ToPlainText()
        {
            return GetNode().ToPlainText();
        }

        public IList<INode> GetChildren()
        {
            return new List<INode>{ GetNode() };
        }

        public void ReplaceChild(int i, INode node)
        {
            if (i != 0) throw new IndexOutOfRangeException();
            Data.Set($"Ref::{Name}", node);
        }

        public bool HasContent()
        {
            return GetNode().HasContent();
        }
    }
}
