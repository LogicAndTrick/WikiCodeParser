using System.Collections.Generic;

namespace LogicAndTrick.WikiCodeParser.Nodes
{
    public interface INode
    {
        /// <summary>
        /// Convert the node to HTML. The resulting output may *not* be sanitised - its possible
        /// malicious input could include injection attacks. Use at your own risk.
        /// </summary>
        string ToHtml();
        string ToPlainText();
        IList<INode> GetChildren();
        void ReplaceChild(int i, INode node);
        bool HasContent();
    }
}