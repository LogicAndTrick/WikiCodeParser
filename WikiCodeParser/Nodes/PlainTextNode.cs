using System.Collections.Generic;

namespace WikiCodeParser.Nodes
{
    /// <summary>
    /// Plain text. Contains only text and no other markup.
    /// </summary>
    public class PlainTextNode : INode
    {
        public static readonly INode NewLine = new PlainTextNode("\n");
        public static readonly INode Empty = new PlainTextNode(string.Empty);

        public string Text { get; set; }

        public PlainTextNode(string text)
        {
            Text = text;
        }

        public string ToHtml() => System.Web.HttpUtility.HtmlEncode(Text);
        public string ToPlainText() => Text;
        public IEnumerable<INode> GetChildren() => new INode[0];
    }
}