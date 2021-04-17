using System;
using System.Collections.Generic;
using System.Linq;
using WikiCodeParser.Nodes;

namespace WikiCodeParser
{
    /// <summary>
    /// WikiCode parser
    /// </summary>
    public class Parser
    {
        public ParserConfiguration Configuration { get; }

        public Parser(ParserConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// Parse WikiCode and return the result.
        /// </summary>
        /// <param name="text">The text to parse</param>
        /// <param name="scope">The scope to parse in</param>
        /// <returns>The parsed result</returns>
        public ParseResult ParseResult(string text, string scope = "")
        {
            return new ParseResult {Content = ParseElements(text, scope)};
        }

        /// <summary>
        /// Parse a block of text and for elements return the resultant nodes.
        /// </summary>
        /// <param name="text">The text to parse</param>
        /// <param name="scope">The scope to parse in</param>
        /// <returns>The nodes of the parsed text</returns>
        internal INode ParseElements(string text, string scope)
        {
            var root = new NodeCollection();

            // Elements are line-based scopes, an element cannot start in the middle of a line.
            text = text.Replace("\r", "");

            var lines = new Lines(text);
            var inscope = Configuration.Elements.Where(x => x.InScope(scope)).OrderByDescending(x => x.Priority).ToList();
            var plain = new List<string>();

            while (lines.Next())
            {
                // Try and find an element for this line
                var matched = false;
                foreach (var e in inscope)
                {
                    if (!e.Matches(lines)) continue;

                    var con = e.Consume(this, lines); // found an element, generate the result
                    if (con == null) continue; // no result, guess this element wasn't valid after all

                    // if we have any plain text, create a node for it
                    if (plain.Count > 0) root.Nodes.Add(ParseTags(String.Join("\n", plain), scope, "block"));
                    plain.Clear();

                    root.Nodes.Add(con);
                    matched = true;
                    break;
                }

                if (!matched) plain.Add(lines.Value()); // there wasn't any match, so this line was plain text
            }

            // parse any plain text that might be left
            if (plain.Count > 0) root.Nodes.Add(ParseTags(String.Join("\n", plain), scope, "block"));

            return root;
        }

        /// <summary>
        /// Parse a block of text for tags and return the resultant node
        /// </summary>
        /// <param name="text">The text to parse</param>
        /// <param name="scope">The scope to parse in</param>
        /// <param name="type">The type of tags to parse - block or inline</param>
        /// <returns>The node of the parsed text</returns>
        internal INode ParseTags(string text, string scope, string type)
        {
            var state = new State(text);
            var root = new NodeCollection();
            var inscope = Configuration.Tags.Where(x => x.InScope(scope)).OrderByDescending(x => x.Priority).ToList();

            while (!state.Done)
            {
                var plain = ParseTextProcessors(state.ScanTo("["), scope, type);
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
                    plain = ParseTextProcessors(state.Next().ToString(), scope, type);
                    if (plain != null) root.Nodes.Add(plain);
                }
            }

            return root;
        }

        /// <summary>
        /// Run text processors and return the resulting node.
        /// </summary>
        /// <param name="text">The text to parse</param>
        /// <param name="scope">The scope to parse in</param>
        /// <param name="type">The type of tags to parse - block or inline</param>
        /// <returns>The node of the parsed text</returns>
        internal INode ParseTextProcessors(string text, string scope, string type)
        {
            if (string.IsNullOrEmpty(text)) return null;
            // todo
            return new PlainTextNode(text);
        }
    }
}