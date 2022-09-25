using System;
using System.Collections.Generic;
using System.Linq;
using LogicAndTrick.WikiCodeParser.Nodes;

namespace LogicAndTrick.WikiCodeParser.Elements
{
    public class MdCodeElement : Element
    {
        private static readonly string[] AllowedLanguages =
        {
            "php", "dos", "bat", "cmd", "css", "cpp", "c", "c++", "cs", "ini", "json", "xml", "html", "angelscript",
            "javascript", "js", "plaintext"
        };

        public MdCodeElement()
        {
            Priority = 10;
        }

        public override bool Matches(Lines lines)
        {
            var value = lines.Value();
            return value.StartsWith("```");
        }

        public override INode Consume(Parser parser, ParseData data, Lines lines, string scope)
        {
            var current = lines.Current();
            var firstLine = lines.Value().Substring(3).TrimEnd();

            string lang = null;
            if (AllowedLanguages.Contains(firstLine, StringComparer.InvariantCultureIgnoreCase))
            {
                lang = firstLine;
                firstLine = "";
            }

            var arr = new List<string> {firstLine};

            var found = false;
            while (lines.Next())
            {
                var value = lines.Value().TrimEnd();
                if (value.EndsWith("```"))
                {
                    var lastLine = value.Substring(0, value.Length - 3);
                    arr.Add(lastLine);
                    found = true;
                    break;
                }
                else
                {
                    arr.Add(value);
                }
            }

            if (!found)
            {
                lines.SetCurrent(current);
                return null;
            }

            // Trim blank lines from the start and end of the array
            for (var i = 0; i < 2; i++)
            {
                while (arr.Count > 0 && arr[0].Trim() == "") arr.RemoveAt(0);
                arr.Reverse();
            }

            // Replace all tabs with 4 spaces
            arr = arr.Select(x => x.Replace("\t", "    ")).ToList();

            // Find the longest common whitespace amongst all lines (ignore blank lines)
            var longestWhitespace = arr.Aggregate(9999, (c, i) =>
            {
                if (i.Trim().Length == 0) return c;
                var wht = i.Length - i.TrimStart().Length;
                return Math.Min(wht, c);
            });

            // Dedent all lines by the longest common whitespace
            arr = arr.Select(a => a.Substring(Math.Min(longestWhitespace, a.Length))).ToList();

            var plain = new UnprocessablePlainTextNode(String.Join("\n", arr));
            var cls = string.IsNullOrWhiteSpace(lang) ? "" : $" class=\"lang-{lang}\"";
            var before = $"<pre{cls}><code>";
            var after = "</code></pre>";
            return new HtmlNode(before, plain, after);
        }
    }
}