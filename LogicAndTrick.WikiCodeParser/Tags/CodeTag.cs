using System.Collections.Generic;
using LogicAndTrick.WikiCodeParser.Nodes;

namespace LogicAndTrick.WikiCodeParser.Tags
{
    public class CodeTag : Tag
    {
        public CodeTag()
        {
            Token = "code";
            Element = "code";
        }

        public override INode FormatResult(Parser parser, ParseData data, State state, string scope, Dictionary<string, string> options, string text)
        {
            return new HtmlNode("<code>", new UnprocessablePlainTextNode(text), "</code>");
        }
    }
}
