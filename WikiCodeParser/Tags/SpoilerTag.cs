using System;
using System.Collections.Generic;
using System.Text;
using WikiCodeParser.Nodes;

namespace WikiCodeParser.Tags
{
    public class SpoilerTag : Tag
    {
        public override string Token => "spoiler";
        public override string Element => "span";
        public override string ElementClass => "spoiler";
        public override string MainOption => "text";
        public override string[] Options => new[] {"text"};
        public override bool AllOptionsInMain => true;

        public override INode FormatResult(Parser parser, State state, string scope, Dictionary<string, string> options, string text)
        {
            var visibleText = "Spoiler";
            if (options.ContainsKey("text") && options["text"].Length > 0) visibleText = options["text"];

            var before = "<" + Element;
            if (ElementClass != null) before += " class=\"" + ElementClass + '"';
            before += $" title=\"{visibleText}\">";
            var after = "</" + Element + '>';
            return new HtmlNode(before, new SpoilerNode(visibleText, parser.ParseTags(text, scope, IsBlock ? "block" : "inline")), after);
        }

        public class SpoilerNode : INode
        {
            public string VisibleText { get; }
            public INode SpoilerContent { get; }

            public SpoilerNode(string visibleText, INode spoilerContent)
            {
                VisibleText = visibleText;
                SpoilerContent = spoilerContent;
            }

            public string ToHtml()
            {
                return SpoilerContent.ToHtml();
            }

            public string ToPlainText()
            {
                return $"[{VisibleText}]";
            }

            public IEnumerable<INode> GetChildren()
            {
                yield return SpoilerContent;
            }
        }
    }
}
