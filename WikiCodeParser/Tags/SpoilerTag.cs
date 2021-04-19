using System.Collections.Generic;
using WikiCodeParser.Nodes;

namespace WikiCodeParser.Tags
{
    public class SpoilerTag : Tag
    {
        public SpoilerTag()
        {
            Token = "spoiler";
            Element = "span";
            ElementClass = "spoiler";
            MainOption = "text";
            Options = new[] {"text"};
            AllOptionsInMain = true;
        }

        public override INode FormatResult(Parser parser, ParseData data, State state, string scope, Dictionary<string, string> options, string text)
        {
            var visibleText = "Spoiler";
            if (options.ContainsKey("text") && options["text"].Length > 0) visibleText = options["text"];

            var before = "<" + Element;
            if (ElementClass != null) before += " class=\"" + ElementClass + '"';
            before += $" title=\"{visibleText}\">";
            var after = "</" + Element + '>';
            return new HtmlNode(before, new SpoilerNode(visibleText, parser.ParseTags(data, text, scope, IsBlock ? "block" : "inline")), after);
        }

        private class SpoilerNode : INode
        {
            private string VisibleText { get; }
            private INode SpoilerContent { get; }

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
                return $"[{VisibleText}](spoiler text)";
            }

            public IEnumerable<INode> GetChildren()
            {
                yield return SpoilerContent;
            }
        }
    }
}
