using System;
using System.Collections.Generic;
using System.Text;
using WikiCodeParser.Nodes;

namespace WikiCodeParser.Elements
{
    public class MdLineElement : Element
    {
        public override bool Matches(Lines lines)
        {
            var value = lines.Value().TrimEnd();
            return value.Length >= 3 && value == new string('-', value.Length);
        }

        public override INode Consume(Parser parser, ParseData data, Lines lines, string scope)
        {
            return new HtmlNode("<hr />", PlainTextNode.Empty, "");
        }
    }
}
