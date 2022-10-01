using System.Collections.Generic;
using LogicAndTrick.WikiCodeParser.Nodes;

namespace LogicAndTrick.WikiCodeParser.Tags
{
    public class VaultEmbedTag : Tag
    {
        public VaultEmbedTag()
        {
            Element = "div";
            MainOption = "id";
            Options = new[] {"id"};
        }

        public override bool Matches(State state, string token)
        {
            var peekTag = state.Peek(7);
            var pt = state.PeekTo("]");
            return peekTag == "[vault:" && pt?.Length > 7 && !pt.Contains("\n");
        }

        public override INode Parse(Parser parser, ParseData data, State state, string scope)
        {
            var index = state.Index;

            if (state.ScanTo(":") != "[vault" || state.Next() != ':')
            {
                state.Seek(index, true);
                return null;
            }

            var str = state.ScanTo("]");
            if (state.Next() != ']')
            {
                state.Seek(index, true);
                return null;
            }

            if (!int.TryParse(str, out var id))
            {
                state.Seek(index, true);
                return null;
            }

            var classes = new List<string> {"embedded", "vault"};
            if (ElementClass != null) classes.Add(ElementClass);

            state.SkipWhitespace();

            var before = $"<div class=\"{string.Join(" ", classes)}\">" +
                         $"<div class=\"embed-container\">" +
                         $"<div class=\"embed-content\">" +
                         $"<div class=\"uninitialised\" data-embed-type=\"vault\" data-vault-id=\"{id}\">" +
                         $"Loading embedded content: Vault Item #{id}";
            var after = "</div></div></div></div>";
            return new HtmlNode(before, PlainTextNode.Empty, after)
            {
                PlainBefore = $"[TWHL vault item #{id}]",
                PlainAfter = "\n",
                IsBlockNode = true
            };
        }
    }
}
