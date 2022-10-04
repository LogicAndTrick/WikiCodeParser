using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using LogicAndTrick.WikiCodeParser.Nodes;

namespace LogicAndTrick.WikiCodeParser.Elements
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

            var contents = parser.ParseTags(data, text, scope, TagParseContext.Inline);
            contents = parser.RunProcessors(contents, data, scope);
            var id = GetUniqueAnchor(data, contents.ToPlainText());
            return new HeadingNode(level, id, contents);
        }

        private static string GetUniqueAnchor(ParseData data, string text)
        {
            const string key = nameof(MdHeadingElement) + ".IdList";
            var anchors = data.Get(key, () => new HashSet<string>());

            var id = Regex.Replace(text, @"[^\da-z?/:@\-._~!$&\'()*+,;=]", "_", RegexOptions.IgnoreCase);
            var anchor = id;
            var inc = 1;
            do {
                // Increment if we have a duplicate
                if (!anchors.Contains(anchor)) break;
                inc++;
                anchor = $"{id}_{inc}";
            } while (true);

            anchors.Add(anchor);
            return anchor;
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

            public bool HasContent()
            {
                return true;
            }
        }
    }
}
