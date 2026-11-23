using System.Collections.Generic;
using LogicAndTrick.WikiCodeParser.Nodes;

namespace LogicAndTrick.WikiCodeParser.Processors
{
    public class MentionsProcessor : INodeProcessor
    {
        public int Priority { get; set; } = 12;
        
        public string UrlFormatString { get; set; }

        public MentionsProcessor(string urlFormatString)
        {
            UrlFormatString = urlFormatString;
        }

        public bool ShouldProcess(INode node, string scope)
        {
            return node is PlainTextNode ptn && ptn.Text.IndexOf('@') >= 0;
        }

        public IEnumerable<INode> Process(Parser parser, ParseData data, INode parentNode, INode node, string scope)
        {
            var text = ((PlainTextNode) node).Text;

            /**
             * mention format = @<name>
             * name cannot contain newlines or the '>' character
             * name length must be between 1 and 64
             * name cannot start or end with whitespace
             */
            
            var state = new State(node.Line, node.Column, text);
            var plainPos = state.SourcePosition;
            var plain = "";
            while (!state.Done)
            {
                plain += state.ScanTo("@<");
                if (state.Done || state.Peek(2) != "@<") continue;

                var tagStart = state.SourcePosition;
                state.Seek(2, false); // @<

                var textStart = state.SourcePosition;
                var name = state.ScanTo(">");
                if (state.Done || name.IndexOf('\n') >= 0 || state.Peek(1) != ">" || name.Length < 1 || name.Length > 64 || name.Length != Util.Trim(name).Length)
                {
                    plain += "@<" + name;
                    continue;
                }

                state.Seek(1, false); // >
                
                if (plain.Length > 0) yield return new PlainTextNode(plainPos, plain);

                yield return new MetadataNode(tagStart, "Mention", name);
                yield return HtmlNodeBuilder.Make("a", tagStart)
                    .AddClass("mention")
                    .Attr("data-name", name)
                    .Attr("href", parser.Configuration.BuildUrl(string.Format(UrlFormatString, HtmlHelper.UrlEncode(name))))
                    .Add(new UnprocessablePlainTextNode(textStart, "@" + name))
                    .ToNode();

                plain = "";
                plainPos = state.SourcePosition;
            }

            plain += state.ScanToEnd();
            if (plain.Length > 0) yield return new PlainTextNode(plainPos, plain);
        }
    }
}