using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using LogicAndTrick.WikiCodeParser.Nodes;

namespace LogicAndTrick.WikiCodeParser.Elements
{
    public class MdTableElement : Element
    {
        public override bool Matches(Lines lines)
        {
            var value = Util.TrimEnd(lines.Value());
            return value.Length >= 2 && value[0] == '|' && (value[1] == '=' || value[1] == '-');
        }

        public override INode Consume(Parser parser, ParseData data, Lines lines, string scope)
        {
            var arr = new List<TableRow>();
            do
            {
                var value = Util.TrimEnd(lines.Value());
                if (value.Length < 2 || value[0] != '|' || (value[1] != '=' && value[1] != '-')) {
                    lines.Back();
                    break;
                }
                var type = value[1] == '=' ? "th" : "td";
                var cells = SplitTable(value.Substring(2)).Select(x => new TableCell(type, ResolveCell(x, parser, data, scope)));
                arr.Add(new TableRow(cells));
            } while (lines.Next());

            return new HtmlNode("<div class=\"table-responsive\"><table class=\"table table-bordered\">", new NodeCollection(arr), "</table></div>");
        }
        
        private static IEnumerable<string> SplitTable(string text)
        {
            var level = 0;
            var last = 0;
            text = Util.Trim(text);
            var len = text.Length;
            int i;
            for (i = 0; i < len; i++)
            {
                var c = text[i];
                if (c == '[') level++;
                else if (c == ']')
                {
                    if (level > 0) level--;
                }
                else if ((c == '|' && level == 0) || i == len - 1)
                {
                    yield return Util.Trim(text.Substring(last, (i - last) + (i == len - 1 ? 1 : 0)));
                    last = i + 1;
                }
            }
            if (last < len) yield return Util.Trim(text.Substring(last, (i-last) + (i == len - 1 ? 1 : 0)));
        }

        private static INode ResolveCell(string text, Parser parser, ParseData data, string scope)
        {
            var res = Regex.Match(Util.Trim(text), "^:ref=([a-z0-9 ]+)$", RegexOptions.IgnoreCase);
            if (res.Success)
            {
                var name = res.Groups[1].Value;
                return new RefNode(data, name);
            }
            return parser.ParseTags(data, text, scope, TagParseContext.Block);
        }

        private class TableRow : INode
        {
            public List<TableCell> Cells { get; }

            public TableRow(IEnumerable<TableCell> cells)
            {
                Cells = cells.ToList();
            }

            public string ToHtml()
            {
                var sb = new StringBuilder("<tr>\n");
                foreach (var cell in Cells)
                {
                    sb.Append(cell.ToHtml()).Append("\n");
                }
                sb.Append("</tr>\n");
                return sb.ToString();
            }

            public string ToPlainText()
            {
                var sb = new StringBuilder();
                var first = true;
                foreach (var cell in Cells)
                {
                    if (!first) sb.Append(" | ");
                    sb.Append(cell.ToPlainText());
                    first = false;
                }
                sb.Append("\n");
                return sb.ToString();
            }

            public IList<INode> GetChildren()
            {
                var list = new List<INode>();
                list.AddRange(Cells);
                return list;
            }

            public void ReplaceChild(int i, INode node)
            {
                Cells[i] = (TableCell) node;
            }

            public bool HasContent()
            {
                return true;
            }
        }

        private class TableCell : INode
        {
            public string Type { get; }
            public INode Contents { get; set; }

            public TableCell(string type, INode contents)
            {
                Type = type;
                Contents = contents;
            }

            public string ToHtml()
            {
                var sb = new StringBuilder();
                sb.Append("<").Append(Type).Append(">");
                sb.Append(Contents.ToHtml());
                sb.Append("</").Append(Type).Append(">");
                return sb.ToString();
            }

            public string ToPlainText()
            {
                return Contents.ToPlainText();
            }

            public IList<INode> GetChildren()
            {
                return new List<INode> { Contents };
            }

            public void ReplaceChild(int i, INode node)
            {
                if (i != 0) throw new ArgumentOutOfRangeException();
                Contents = node;
            }

            public bool HasContent()
            {
                return Contents.HasContent();
            }
        }
    }
}
