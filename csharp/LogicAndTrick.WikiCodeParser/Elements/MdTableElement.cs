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
            var startLine = lines.SourceLine;
            var arr = new List<TableRow>();
            do
            {
                var value = lines.Value();
                if (value.Length < 2 || value[0] != '|' || (value[1] != '=' && value[1] != '-')) {
                    lines.Back();
                    break;
                }
                var line = lines.SourceLine;
                var type = value[1] == '=' ? "th" : "td";
                var cells = SplitTable(2, value).Select(x => new TableCell(line, x.col, type, ResolveCell(x.text, parser, data, scope, line, x.col)));
                arr.Add(new TableRow(line, 0, cells));
            } while (lines.Next());

            return new HtmlNode(new Position(startLine, 0), "<div class=\"table-responsive\"><table class=\"table table-bordered\">", new NodeCollection(arr), "</table></div>");
        }
        
        private static IEnumerable<(int col, string text)> SplitTable(int start, string text)
        {
            var level = 0;
            var last = start;
            var len = text.Length;
            int i;
            for (i = start; i < len; i++)
            {
                var c = text[i];
                if (c == '[') level++;
                else if (c == ']')
                {
                    if (level > 0) level--;
                }
                else if ((c == '|' && level == 0) || i == len - 1)
                {
                    yield return CreateTableText(text, last, i);
                    last = i + 1;
                }
            }
            if (last < len) yield return CreateTableText(text, last, i);
        }

        private static (int col, string text) CreateTableText(string text, int start, int end)
        {
            // get the text length
            var col = start;
            var txt = text.Substring(start, (end - start) + (end == text.Length - 1 ? 1 : 0));

            // trim the start of the text - the number of removed chars is how much we need to add to the column
            var ol = txt.Length;
            var ts = Util.TrimStart(txt);
            col += ol - ts.Length;

            return (col, Util.TrimEnd(ts));
        }

        private static INode ResolveCell(string text, Parser parser, ParseData data, string scope, int line, int column)
        {
            var res = Regex.Match(text, "^:ref=([a-z0-9 ]+)$", RegexOptions.IgnoreCase);
            if (res.Success)
            {
                var name = res.Groups[1].Value;
                return new RefNode(data, name);
            }
            return parser.ParseTags(data, text, scope, TagParseContext.Block, line, column);
        }

        private class TableRow : INode
        {
            public int Line { get; set; }
            public int Column { get; set; }
            public List<TableCell> Cells { get; }

            public TableRow(int line, int column, IEnumerable<TableCell> cells)
            {
                Line = line;
                Column = column;
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
            public int Line { get; set; }
            public int Column { get; set; }
            public string Type { get; }
            public INode Contents { get; set; }

            public TableCell(int line, int column, string type, INode contents)
            {
                Line = line;
                Column = column;
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
