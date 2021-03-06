using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using WikiCodeParser.Nodes;

namespace WikiCodeParser.Elements
{
    public class MdHeadingElement : Element
    {
        public override bool Matches(Lines lines)
        {
            var value = lines.Value();
            return value.Length > 0 && value[0] == '=';
        }

        public override INode Consume(Parser parser, ParseData data, Lines lines, string scope)
        {
            var value = lines.Value().Trim();
            var res = Regex.Match(value, @"^(=+)(.*?)=*$", RegexOptions.IgnoreCase);
            var level = Math.Min(6, res.Groups[1].Value.Length);
            var text = res.Groups[2].Value.Trim();

            var id = GenerateUniqueHeaderID(data, text);
            return new HeadingNode(level, id, parser.ParseTags(data, text, scope, "inline"));
        }

        private static string GenerateUniqueHeaderID(ParseData data, string text)
        {
            text = text.ToLower().Replace(' ', '-');
            text = Regex.Replace(text, @"[^0-9a-z\-]", "");
            text = Regex.Replace(text, @"\-{2,}", "-");
            if (text.Length < 3) text = "h" + Guid.NewGuid().ToString("N").Substring(6, 12).ToLower();
            if (Char.IsDigit(text[0])) text = "_" + text;

            const string key = nameof(MdHeadingElement) + ".IdList";
            var list = data.Get(key, () => new List<string>());
            var id = text;
            var num = 2;
            while (list.Contains(id)) id = text + "-" + num++;
            list.Add(id);
            return id;
        }

        public class HeadingNode : INode
        {
            public int Level { get; }
            public string ID { get; }
            public INode Text { get; set; }

            public HeadingNode(int level, string id, INode text)
            {
                Level = level;
                ID = id;
                Text = text;
            }

            public string ToHtml()
            {
                return $"<h{Level} id=\"{ID}\">"
                       + Text.ToHtml()
                       + $"</h{Level}>";
            }

            public string ToPlainText()
            {
                var plain = Text.ToPlainText();
                return plain + "\n" + new string('-', plain.Length);
            }

            public IList<INode> GetChildren()
            {
                return new[] { Text };
            }

            public void ReplaceChild(int i, INode node)
            {
                if (i != 0) throw new ArgumentOutOfRangeException();
                Text = node;
            }
        }
    }
}
