using System;
using System.Collections.Generic;
using System.Linq;
using LogicAndTrick.WikiCodeParser.Nodes;

namespace LogicAndTrick.WikiCodeParser.Elements
{
    public class MdColumnsElement : Element
    {
        public override bool Matches(Lines lines)
        {
            var value = lines.Value();
            return value.StartsWith("%%columns=");
        }

        public override INode Consume(Parser parser, ParseData data, Lines lines, string scope)
        {
            var current = lines.Current();

            var meta = lines.Value().Substring(10);
            var colDefs = meta.Split(':').Select(x => int.TryParse(x, out var v) ? v : 0).ToList();
            var total = 0;

            foreach (var d in colDefs)
            {
                if (d > 0)
                {
                    total += d;
                }
                else
                {
                    lines.SetCurrent(current);
                    return null;
                }
            }

            if (total != 12)
            {
                lines.SetCurrent(current);
                return null;
            }

            var i = 0;

            var arr = new List<string>();
            var cols = new List<ColumnNode>();
            while (lines.Next() && i < colDefs.Count)
            {
                var value = lines.Value().TrimEnd();
                if (value == "%%")
                {
                    cols.Add(new ColumnNode(colDefs[i], parser.ParseElements(data, String.Join("\n", arr), scope)));
                    arr.Clear();
                    i++;
                } else {
                    arr.Add(value);
                }
                if (i >= colDefs.Count) break;
            }

            if (i != colDefs.Count || arr.Count > 0)
            {
                lines.SetCurrent(current);
                return null;
            }

            return new HtmlNode("<div class=\"row\">", new NodeCollection(cols), "</div>");
        }

        public class ColumnNode : INode
        {
            public int Width { get; }
            public INode Content { get; set; }

            public ColumnNode(int width, INode content)
            {
                Width = width;
                Content = content;
            }

            public string ToHtml()
            {
                return $"<div class=\"col-md-{Width}\">\n" + Content.ToHtml() + "</div>\n";
            }

            public string ToPlainText()
            {
                return Content.ToPlainText();
            }

            public IList<INode> GetChildren()
            {
                return new[] {Content};
            }

            public void ReplaceChild(int i, INode node)
            {
                if (i != 0) throw new ArgumentOutOfRangeException();
                Content = node;
            }
        }
    }
}
