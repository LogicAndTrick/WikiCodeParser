﻿using System.Collections.Generic;
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
            var value = lines.Value().TrimEnd();
            return value.Length >= 2 && value[0] == '|' && (value[1] == '=' || value[1] == '-');
        }

        public override INode Consume(Parser parser, ParseData data, Lines lines, string scope)
        {
            var arr = new List<TableRow>();
            do
            {
                var value = lines.Value().TrimEnd();
                if (value.Length < 2 || value[0] != '|' || (value[1] != '=' && value[1] != '-')) {
                    lines.Back();
                    break;
                }
                var cells = SplitTable(value.Substring(2)).Select(x => ResolveCell(x, parser, data, scope));
                arr.Add(new TableRow(value[1] == '=' ? "th" : "td", cells));
            } while (lines.Next());

            return new HtmlNode("<div class=\"table-responsive\"><table class=\"table table-bordered\">", new NodeCollection(arr), "</table></div>");
        }
        
        private static IEnumerable<string> SplitTable(string text)
        {
            var level = 0;
            var last = 0;
            text = text.Trim();
            var len = text.Length;
            int i;
            for (i = 0; i < len; i++)
            {
                var c = text[i];
                if (c == '[') level++;
                else if (c == ']') level--;
                else if ((c == '|' && level == 0) || i == len - 1)
                {
                    yield return text.Substring(last, (i - last) + (i == len - 1 ? 1 : 0)).Trim();
                    last = i + 1;
                }
            }
            if (last < len) yield return text.Substring(last, (i-last) + (i == len - 1 ? 1 : 0)).Trim();
        }

        private static INode ResolveCell(string text, Parser parser, ParseData data, string scope)
        {
            var res = Regex.Match(text.Trim(), "^:ref=([a-z0-9 ]+)$", RegexOptions.IgnoreCase);
            if (res.Success)
            {
                var name = res.Groups[1].Value;
                return new RefNode(data, name);
            }
            return parser.ParseTags(data, text, scope, TagParseContext.Block);
        }

        private class TableRow : INode
        {
            public string Type { get; }
            public List<INode> Cells { get; }

            public TableRow(string type, IEnumerable<INode> cells)
            {
                Type = type;
                Cells = cells.ToList();
            }

            public string ToHtml()
            {
                var sb = new StringBuilder("<tr>\n");
                foreach (var cell in Cells)
                {
                    sb.Append("<").Append(Type).Append(">");
                    sb.Append(cell.ToHtml());
                    sb.Append("</").Append(Type).Append(">\n");
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
                return Cells;
            }

            public void ReplaceChild(int i, INode node)
            {
                Cells[i] = node;
            }

            public bool HasContent()
            {
                return true;
            }
        }
    }
}
