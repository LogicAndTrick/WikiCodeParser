using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using LogicAndTrick.WikiCodeParser.Nodes;
using LogicAndTrick.WikiCodeParser.Processors;

namespace LogicAndTrick.WikiCodeParser
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
            var data = new ParseData();
            text = text.Trim();
            var node = ParseElements(data, text, scope);
            node = RunProcessors(node, data, scope);
            return new ParseResult
            {
                Content = node
            };
        }

        /// <summary>
        /// Parse a block of text and for elements return the resultant nodes.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="text">The text to parse</param>
        /// <param name="scope">The scope to parse in</param>
        /// <returns>The nodes of the parsed text</returns>
        internal INode ParseElements(ParseData data, string text, string scope)
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

                    var con = e.Consume(this, data, lines, scope); // found an element, generate the result
                    if (con == null) continue; // no result, guess this element wasn't valid after all

                    // if we have any plain text, create a node for it
                    if (plain.Count > 0)
                    {
                        root.Nodes.Add(TrimWhitespace(ParseTags(data, String.Join("\n", plain).Trim(), scope, "block")));
                        root.Nodes.Add(UnprocessablePlainTextNode.NewLine); // Newline before next element
                    }
                    plain.Clear();

                    root.Nodes.Add(con);
                    root.Nodes.Add(UnprocessablePlainTextNode.NewLine); // Elements always have a newline after
                    matched = true;
                    break;
                }

                if (!matched) plain.Add(lines.Value()); // there wasn't any match, so this line was plain text
            }

            // parse any plain text that might be left
            if (plain.Count > 0) root.Nodes.Add(TrimWhitespace(ParseTags(data, String.Join("\n", plain).Trim(), scope, "block")));
            
            // Trim off any whitespace nodes at the end
            while (root.Nodes.Count > 0 && root.Nodes[root.Nodes.Count - 1] is UnprocessablePlainTextNode ptn && string.IsNullOrWhiteSpace(ptn.Text))
            {
                root.Nodes.RemoveAt(root.Nodes.Count - 1);
            }

            FlattenNestedNodeCollections(root);
            return TrimWhitespace(root);
        }

        // ReSharper disable once MemberCanBePrivate.Global
        internal static INode TrimWhitespace(INode node, bool start = true, bool end = true)
        {
            var removeNodes = new List<INode>();

            if (start)
            {
                node.Walk(x =>
                {
                    if (x is NodeCollection) return true;
                    if (x.HasContent()) return false;
                    if (x is UnprocessablePlainTextNode || x is PlainTextNode) removeNodes.Add(x);
                    return true;
                });
            }

            if (end)
            {
                node.WalkBack(x =>
                {
                    if (x is NodeCollection) return true;
                    if (x.HasContent()) return false;
                    if (x is UnprocessablePlainTextNode || x is PlainTextNode) removeNodes.Add(x);
                    return true;
                });
            }

            foreach (var rem in removeNodes)
            {
                node.Remove(rem);
            }

            return node;
        }

        /// <summary>
        /// Parse a block of text for tags and return the resultant node
        /// </summary>
        /// <param name="data"></param>
        /// <param name="text">The text to parse</param>
        /// <param name="scope">The scope to parse in</param>
        /// <param name="type">The type of tags to parse - block or inline</param>
        /// <returns>The node of the parsed text</returns>
        internal INode ParseTags(ParseData data, string text, string scope, string type)
        {
            // trim 3 or more newlines down to 2 newlines
            text = Regex.Replace(text, "\n{3,}", "\n\n");

            var state = new State(text);
            var root = new NodeCollection();
            var inscope = Configuration.Tags.Where(x => x.InScope(scope)).OrderByDescending(x => x.Priority).ToList();

            while (!state.Done)
            {
                var plain = state.ScanTo("[");
                if (!String.IsNullOrEmpty(plain)) root.Nodes.Add(new PlainTextNode(plain));
                if (state.Done) break;

                var token = state.GetToken();
                var found = false;
                foreach (var t in inscope)
                {
                    if (t.Matches(state, token))
                    {
                        var parsed = t.Parse(this, data, state, scope);
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
                    plain = state.Next().ToString();
                    if (!String.IsNullOrEmpty(plain)) root.Nodes.Add(new PlainTextNode(plain));
                }
            }

            return root;
        }

        internal void FlattenNestedNodeCollections(INode node)
        {
            if (node is NodeCollection coll)
            {
                while (coll.Nodes.Any(x => x is NodeCollection))
                {
                    coll.Nodes = coll.Nodes.SelectMany(x => x is NodeCollection nc ? nc.Nodes : new List<INode> { x }).ToList();
                }
            }

            foreach (var child in node.GetChildren())
            {
                FlattenNestedNodeCollections(child);
            }
        }

        internal INode RunProcessors(INode node, ParseData data, string scope)
        {
            foreach (var processor in Configuration.Processors.OrderByDescending(x => x.Priority))
            {
                node = RunProcessor(node, processor, data, scope);
            }

            return node;
        }

        internal INode RunProcessor(INode node, INodeProcessor processor, ParseData data, string scope)
        {
            // If the node can be processed, don't touch subnodes - the processor can invoke RunProcessor if it's needed.
            if (processor.ShouldProcess(node, scope))
            {
                var result = processor.Process(this, data, node, scope).ToList();
                return result.Count == 1 ? result[0] : new NodeCollection(result);
            }

            var children = node.GetChildren();

            for (var i = 0; i < children.Count; i++)
            {
                var child = children[i];
                var processed = RunProcessor(child, processor, data, scope);
                node.ReplaceChild(i, processed);
            }

            return node;
        }
    }
}