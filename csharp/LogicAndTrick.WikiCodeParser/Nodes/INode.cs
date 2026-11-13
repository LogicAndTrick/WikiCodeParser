using System.Collections.Generic;

namespace LogicAndTrick.WikiCodeParser.Nodes
{
    public interface INode
    {
        /// <summary>
        /// The index of the line (1-indexed) in the source input where this node starts.
        /// If the line is 0, then the position is not set.
        /// </summary>
        int Line { get; }

        /// <summary>
        /// The index of the column (0-indexed) in the source input where this node starts.
        /// </summary>
        int Column { get; }

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