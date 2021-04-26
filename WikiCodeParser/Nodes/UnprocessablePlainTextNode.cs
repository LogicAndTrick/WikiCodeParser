using System;
using System.Collections.Generic;
using System.Text;

namespace WikiCodeParser.Nodes
{
    public class UnprocessablePlainTextNode : INode
    {
        public static readonly INode Empty = new UnprocessablePlainTextNode(string.Empty);
        public static readonly INode NewLine = new UnprocessablePlainTextNode("\n");

        public string Text { get; set; }

        public UnprocessablePlainTextNode(string text)
        {
            Text = text;
        }

        public string ToHtml() => System.Web.HttpUtility.HtmlEncode(Text);
        public string ToPlainText() => Text;
        public IList<INode> GetChildren() => new INode[0];
        public void ReplaceChild(int i, INode node) => throw new InvalidOperationException();
    }
}
