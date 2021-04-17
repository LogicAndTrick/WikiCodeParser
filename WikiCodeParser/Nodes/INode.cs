using System.Collections.Generic;

namespace WikiCodeParser.Nodes
{
    public interface INode
    {
        string ToHtml();
        string ToPlainText();
        IEnumerable<INode> GetChildren();
    }
}