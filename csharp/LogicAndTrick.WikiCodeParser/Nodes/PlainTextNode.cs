using System;
using System.Collections.Generic;

namespace LogicAndTrick.WikiCodeParser.Nodes
{
    /// <summary>
    /// Plain text. Contains only text and no other markup.
    /// </summary>
    public class PlainTextNode : INode
    {
        public static INode Empty => new PlainTextNode(string.Empty);

        public string Text { get; set; }

        public PlainTextNode(string text)
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