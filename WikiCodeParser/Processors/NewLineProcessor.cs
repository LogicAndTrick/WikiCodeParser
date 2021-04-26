using System.Collections.Generic;
using System.Text.RegularExpressions;
using WikiCodeParser.Nodes;

namespace WikiCodeParser.Processors
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
            var text = ((PlainTextNode) node).Text;
            text = Regex.Replace(text, " *<br> *", "\n");

            var lines = text.Split('\n');
            var prevLineIsBlank = false;
            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                if (string.IsNullOrEmpty(line))
                {
                    if (!prevLineIsBlank) yield return new HtmlNode("<br/>", UnprocessablePlainTextNode.NewLine, "");
                    prevLineIsBlank = true;
                }
                else
                {
                    prevLineIsBlank = false;
                    yield return new PlainTextNode(line);
                    // Don't emit a line break after the final line of the text as it did not end with a newline
                    if (i < lines.Length - 1) yield return new HtmlNode("<br/>", UnprocessablePlainTextNode.NewLine, "");
                }
            }
        }
    }
}
