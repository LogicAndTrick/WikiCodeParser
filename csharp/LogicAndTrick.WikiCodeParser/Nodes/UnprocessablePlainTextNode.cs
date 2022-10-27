using System;
using System.Collections.Generic;

namespace LogicAndTrick.WikiCodeParser.Nodes
{
    public class UnprocessablePlainTextNode : INode
    {
        public static INode Empty => new UnprocessablePlainTextNode(string.Empty);
        public static INode NewLine => new UnprocessablePlainTextNode("\n");

        public string Text { get; set; }

        public UnprocessablePlainTextNode(string text)
        {
            Text = text;
        }

        public string ToHtml() => HtmlHelper.Encode(Text);
        public string ToPlainText() => Text;
        public IList<INode> GetChildren() => Array.Empty<INode>();
        public void ReplaceChild(int i, INode node) => throw new InvalidOperationException();
        public bool HasContent() => !String.IsNullOrWhiteSpace(Text);
    }
}
