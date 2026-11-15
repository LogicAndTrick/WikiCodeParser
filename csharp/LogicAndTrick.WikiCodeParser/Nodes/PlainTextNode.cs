using System;
using System.Collections.Generic;

namespace LogicAndTrick.WikiCodeParser.Nodes
{
    /// <summary>
    /// Plain text. Contains only text and no other markup.
    /// </summary>
    public class PlainTextNode : INode
    {
        public static INode Empty(Position pos) => new PlainTextNode(pos, string.Empty);

        public int Line { get; set; }
        public int Column { get; set; }
        public string Text { get; set; }

        public PlainTextNode(Position pos, string text)
        {
            Line = pos.Line;
            Column = pos.Column;
            Text = text;
        }

        public string ToHtml() => HtmlHelper.Encode(Text);
        public string ToPlainText() => Text;
        public IList<INode> GetChildren() => Array.Empty<INode>();
        public void ReplaceChild(int i, INode node) => throw new InvalidOperationException();
        public bool HasContent() => !String.IsNullOrWhiteSpace(Text);
        public override string ToString() => $"plain{{{Text.Replace("\n", "\\n")}}}";
    }
}