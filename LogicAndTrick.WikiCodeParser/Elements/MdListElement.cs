using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogicAndTrick.WikiCodeParser.Nodes;

namespace LogicAndTrick.WikiCodeParser.Elements
{
    public class MdListElement : Element
    {
        private static readonly HashSet<char> UlTokens = new HashSet<char> { '*', '-' };
        private static readonly HashSet<char> OlTokens = new HashSet<char> { '#' };

        private static bool IsUnsortedToken(char c) => UlTokens.Contains(c);
        private static bool IsSortedToken(char c) => OlTokens.Contains(c);
        private static bool IsListToken(char c) => IsUnsortedToken(c) || IsSortedToken(c);

        private static int IsValidListItem(string value, int currentLevel)
        {
            var len = value.Length;
            if (len == 0) return 0;

            var tokens = 0;
            var foundSpace = false;
            for (var i = 0; i < len; i++)
            {
                var c = value[i];
                if (IsListToken(c))
                {
                    tokens++;
                    continue;
                }

                if (c == ' ')
                {
                    foundSpace = true;
                    break;
                }

                return 0;
            }

            if (foundSpace && tokens > 0 && tokens <= currentLevel + 1) return tokens;
            return 0;
        }

        public override bool Matches(Lines lines)
        {
            var value = lines.Value().Trim();
            return IsValidListItem(value, 0) > 0;
        }

        public override INode Consume(Parser parser, ParseData data, Lines lines, string scope)
        {
            var current = lines.Current();

            // Put all the subtrees into a dummy item node
            var item = new ListItemNode(PlainTextNode.Empty);
            var _ = CreateListItems(item, "", parser, data, lines, scope).ToList();

            if (!item.Subtrees.Any())
            {
                lines.SetCurrent(current);
                return null;
            }

            // Pull the subtrees out again for the result
            if (item.Subtrees.Count == 1) return item.Subtrees[0];
            return new NodeCollection(item.Subtrees);
        }

        /// <summary>
        /// Converts lines into list items for a given prefix until the prefix no longer matches a line
        /// </summary>
        private IEnumerable<ListItemNode> CreateListItems(ListItemNode lastItemNode, string prefix, Parser parser, ParseData data, Lines lines, string scope)
        {
            do
            {
                var value = lines.Value().TrimEnd();

                if (!value.StartsWith(prefix))
                {
                    // No longer valid for this list
                    lines.Back();
                    yield break;
                }

                value = value.Substring(prefix.Length); // strip the prefix off the line

                // Possibilities:
                // empty string : not valid - stop parsing
                // first character is whitespace : trim and create list item
                // first character is list token, second character is whitespace: create sublist
                // anything else : not valid - stop parsing

                if (value.Length > 1 && value[0] == ' ' && prefix.Length > 0) // don't allow this if we're parsing at level 0
                {
                    // List item
                    value = value.TrimStart();

                    // Support for continuations
                    while (value.EndsWith("^"))
                    {
                        if (value.EndsWith("\\^")) // super basic way to escape continuations
                        {
                            value = value.Substring(0, value.Length - 2) + "^";
                            break;
                        }
                        else if (lines.Next())
                        {
                            value = value.Substring(0, value.Length - 1).Trim() + "\n" + lines.Value().TrimStart();
                        }
                        else
                        {
                            break;
                        }
                    }

                    var pt = parser.ParseTags(data, value.Trim(), scope, "inline");
                    lastItemNode = new ListItemNode(pt);
                    yield return lastItemNode;
                }
                else if (value.Length > 2 && IsListToken(value[0]) && value[1] == ' ' && lastItemNode != null)
                {
                    // Sublist
                    var tag = IsSortedToken(value[0]) ? "ol" : "ul";
                    var sublist = new ListNode(tag, CreateListItems(lastItemNode, prefix + value[0], parser, data, lines, scope));
                    lastItemNode.Subtrees.Add(sublist);
                }
                else
                {
                    // Cannot parse this line, list is complete
                    lines.Back();
                    yield break;
                }
            } while (lines.Next());
        }
        
        public class ListNode : INode
        {
            public string Tag { get; }
            public List<ListItemNode> Items { get; }

            public ListNode(string tag, IEnumerable<ListItemNode> items)
            {
                Tag = tag;
                Items = items.ToList();
            }

            public string ToHtml()
            {
                var sb = new StringBuilder();
                sb.Append("<").Append(Tag).Append(">\n");
                foreach (var item in Items) sb.Append(item.ToHtml());
                sb.Append("</").Append(Tag).Append(">\n");
                return sb.ToString();
            }

            public string ToPlainText()
            {
                return ToPlainText("");
            }

            public string ToPlainText(string prefix)
            {
                var st = prefix + (Tag == "ol" ? "#" : "-");
                var sb = new StringBuilder();
                foreach (var item in Items)
                {
                    sb.Append(item.ToPlainText(st));
                }
                return sb.ToString();
            }

            public IList<INode> GetChildren()
            {
                return Items.OfType<INode>().ToList();
            }

            public void ReplaceChild(int i, INode node)
            {
                Items[i] = (ListItemNode) node;
            }
        }

        public class ListItemNode : INode
        {
            public INode Content { get; set; }
            public List<ListNode> Subtrees { get; }

            public ListItemNode(INode content)
            {
                Content = content;
                Subtrees = new List<ListNode>();
            }

            public string ToHtml()
            {
                var sb = new StringBuilder("<li>");
                sb.Append(Content.ToHtml());
                foreach (var st in Subtrees) sb.Append(st.ToHtml());
                sb.Append("</li>\n");
                return sb.ToString();
            }

            public string ToPlainText()
            {
                throw new InvalidOperationException();
            }

            public string ToPlainText(string prefix)
            {
                var sb = new StringBuilder(prefix).Append(" ");
                sb.Append(Content.ToPlainText()).Append("\n");
                foreach (var st in Subtrees) sb.Append(st.ToPlainText(prefix));
                return sb.ToString();
            }

            public IList<INode> GetChildren()
            {
                return new[] {Content}.Concat(Subtrees).ToList();
            }

            public void ReplaceChild(int i, INode node)
            {
                if (i == 0) Content = node;
                else Subtrees[i - 1] = (ListNode) node;
            }
        }
    }
}
