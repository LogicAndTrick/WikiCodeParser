using System;
using System.Collections.Generic;
using System.Text;
using WikiCodeParser.Nodes;

namespace WikiCodeParser.Tags
{
    public class PreTag : Tag
    {
        public override string Token => "pre";
        public override string Element => "pre";

        public override INode FormatResult(Parser parser, State state, string scope, Dictionary<string, string> options, string text)
        {
            var before = "<" + Element;
            if (ElementClass != null) before += " class=\"" + ElementClass + '"';
            before += "><code>";
            var after = "</code></" + Element + '>';
            return new HtmlNode(before, new PlainTextNode(text), after);
        }
    }
}
