using System;
using System.Collections.Generic;

namespace LogicAndTrick.WikiCodeParser.Nodes
{
    public class UnprocessablePlainTextNode : INode
    {
        public static INode Empty(Position pos) => new UnprocessablePlainTextNode(pos, string.Empty);
        public static INode NewLine(Position pos) => new UnprocessablePlainTextNode(pos, "\n");

        public int Line { get; set; }
        public int Column { get; set; }
        public string Text { get; set; }

        public UnprocessablePlainTextNode(Position pos, string text)
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
        public override string ToString() => $"unprocessable{{{Text.Replace("\n", "\\n")}}}";
    }
}
