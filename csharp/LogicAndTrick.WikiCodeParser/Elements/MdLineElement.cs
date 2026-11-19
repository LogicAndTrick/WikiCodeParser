using LogicAndTrick.WikiCodeParser.Nodes;

namespace LogicAndTrick.WikiCodeParser.Elements
{
    public class MdLineElement : Element
    {
        public override bool Matches(Lines lines)
        {
            var value = Util.TrimEnd(lines.Value());
            return value.Length >= 3 && value == new string('-', value.Length);
        }

        public override INode Consume(Parser parser, ParseData data, Lines lines, string scope)
        {
            return new HtmlNode(new Position(lines.SourceLine, 0), "<hr />", PlainTextNode.Empty(new Position(lines.SourceLine, 0)), "")
            {
                PlainBefore = "---"
            };
        }
    }
}
