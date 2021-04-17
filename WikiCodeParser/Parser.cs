using System;
using System.Collections.Generic;
using System.Linq;
using WikiCodeParser.Elements;
using WikiCodeParser.Nodes;
using WikiCodeParser.Tags;

namespace WikiCodeParser
{
    public class Parser
    {
        public List<BBCodeElement> Elements { get; }
        public List<BBCodeTag> Tags { get; }

        public Parser()
        {
            Elements = new List<BBCodeElement>();
            Tags = new List<BBCodeTag>();
        }

        public ParseResult ParseResult(string text, string scope = "")
        {
            var result = new ParseResult();
            result.Content.Nodes.AddRange(ParseBlock(result, text, scope));
            return result;
        }

        public IEnumerable<INode> ParseBlock(ParseResult result, string text, string scope)
        {
            text = text.Replace("\r", "");
            return SplitElements(text, scope);
        }

        public IEnumerable<INode> SplitElements(string text, string scope)
        {
            var lines = new Lines(text);
            var inscope = Elements.Where(x => x.InScope(scope)).OrderByDescending(x => x.Priority).ToList();
            var plain = new List<string>();

            while (lines.Next())
            {
                var matched = false;
                foreach (var e in inscope)
                {
                    if (!e.Matches(lines)) continue;

                    var con = e.Consume(this, lines);
                    if (con == null) continue;

                    if (plain.Count > 0) yield return ParseInline(String.Join("\n", plain), scope, "block");
                    plain.Clear();

                    yield return con;
                    matched = true;
                    break;
                }

                if (!matched) plain.Add(lines.Value());
            }

            if (plain.Count > 0) yield return ParseInline(String.Join("\n", plain), scope, "block");
        }

        public INode ParseInline(string text, string scope, string type)
        {
            var state = new State(text);
            var root = new NodeCollection();
            var inscope = Tags.Where(x => x.InScope(scope)).OrderByDescending(x => x.Priority).ToList();

            while (!state.Done)
            {
                var plain = ParsePlainText(state.ScanTo("["), scope, type);
                if (plain != null) root.Nodes.Add(plain);
                if (state.Done) break;

                var token = state.GetToken();
                var found = false;
                foreach (var t in inscope)
                {
                    if (t.Matches(state, token))
                    {
                        var parsed = t.Parse(this, state, scope);
                        if (parsed != null)
                        {
                            root.Nodes.Add(parsed);
                            found = true;
                            break;
                        }
                    }
                }

                if (!found)
                {
                    plain = ParsePlainText(state.Next().ToString(), scope, type);
                    if (plain != null) root.Nodes.Add(plain);
                }
            }

            return root;
        }

        public INode ParsePlainText(string text, string scope, string type)
        {
            if (string.IsNullOrEmpty(text)) return null;
            // todo
            return new PlainTextNode(text);
        }
    }
}