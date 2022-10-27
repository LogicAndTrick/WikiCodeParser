using System.Collections.Generic;

namespace LogicAndTrick.WikiCodeParser.Nodes
{
    public interface INode
    {
        string ToHtml();
        string ToPlainText();
        IList<INode> GetChildren();
        void ReplaceChild(int i, INode node);
        bool HasContent();
    }
}