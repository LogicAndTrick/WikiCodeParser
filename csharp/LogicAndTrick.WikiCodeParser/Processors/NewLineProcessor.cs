using System.Collections.Generic;
using System.Text.RegularExpressions;
using LogicAndTrick.WikiCodeParser.Nodes;

namespace LogicAndTrick.WikiCodeParser.Processors
{
    public class NewLineProcessor : INodeProcessor
    {
        public int Priority { get; set; } = 1;

        public bool ShouldProcess(INode node, string scope)
        {
            return node is PlainTextNode ptn && (ptn.Text.Contains("\n") || ptn.Text.Contains("<br>"));
        }

        public IEnumerable<INode> Process(Parser parser, ParseData data, INode node, string scope)
        {
            var text = ((PlainTextNode)node).Text;

            var lines = new List<string>();
            foreach (var line in text.Split('\n'))
            {
                var breaks = Regex.Split(line, "<br>");
                for (var i = 0; i < breaks.Length; i++)
                {
                    var s = breaks[i];
                    if (i > 0) s = Util.TrimStart(s);
                    if (i < breaks.Length - 1) s = Util.TrimEnd(s);
                    lines.Add(s);
                }
            }

            for (var i = 0; i < lines.Count; i++)
            {
                var line = lines[i];
                yield return new PlainTextNode(line);
                // Don't emit a line break after the final line of the text as it did not end with a newline
                if (i < lines.Count - 1) yield return new HtmlNode("<br/>", UnprocessablePlainTextNode.NewLine, "");
            }
        }
    }
}
