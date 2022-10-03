using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using LogicAndTrick.WikiCodeParser.Nodes;

namespace LogicAndTrick.WikiCodeParser.Elements
{
    public class PreElement : Element
    {
        private static readonly string[] AllowedLanguages =
        {
            "php", "dos", "bat", "cmd", "css", "cpp", "c", "c++", "cs", "ini", "json", "xml", "html", "angelscript",
            "javascript", "js", "plaintext"
        };

        public override bool Matches(Lines lines)
        {
            var value = lines.Value().Trim();
            return value.Length > 4 && value.Substring(0, 4) == "[pre" && Regex.IsMatch(value, @"\[pre(=[a-z ]+)?\]", RegexOptions.IgnoreCase);
        }

        public override INode Consume(Parser parser, ParseData data, Lines lines, string scope)
        {
            var current = lines.Current();
            var arr = new List<string>();

            var line = lines.Value().Trim();
            var res = Regex.Match(line, @"\[pre(?:=([a-z ]+))?\]", RegexOptions.IgnoreCase);
            if (!res.Success)
            {
                lines.SetCurrent(current);
                return null;
            }

            line = line.Substring(res.Value.Length);
            string lang = null;
            var hl = false;
            if (res.Groups[1].Success) {
                var spl = res.Groups[1].Value.Split(' ');
                hl = spl.Contains("highlight");
                lang = spl.FirstOrDefault(x => x != "highlight");
            }

            if (line.EndsWith("[/pre]"))
            {
                lines.Next();
                arr.Add(line.Substring(0, line.Length - 6));
            }
            else
            {
                if (line.Length > 0) arr.Add(line);
                var found = false;
                while (lines.Next())
                {
                    var value = lines.Value().TrimEnd();
                    if (value.EndsWith("[/pre]"))
                    {
                        var lastLine = value.Substring(0, value.Length - 6);
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
            }

            // Trim blank lines from the start and end of the array
            for (var i = 0; i < 2; i++)
            {
                while (arr.Count > 0 && arr[0].Trim() == "") arr.RemoveAt(0);
                arr.Reverse();
            }

            // Process highlight commands
            var highlight = new List<(int firstLine, int numLines, string color)>();
            if (hl)
            {
                // Highlight commands get their own line so we need to keep track of which lines we're removing as we go
                var newArr = new List<string>();
                var firstLine = 0;
                foreach (var srcLine in arr)
                {
                    if (srcLine.StartsWith("@@"))
                    {
                        var match = Regex.Match(srcLine, @"^@@(?:(#[0-9a-f]{3}|#[0-9a-f]{6}|[a-z]+|\d+)(?::(\d+))?)?$", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                        if (match.Success)
                        {
                            var numLines = 1;
                            var color = "#FF8000";
                            for (var i = 1; i < match.Groups.Count; i++)
                            {
                                var p = match.Groups[i].Value;
                                if (Colours.IsValidColor(p)) color = p;
                                else if (int.TryParse(p, out var iv)) numLines = iv;
                            }
                            highlight.Add((firstLine, numLines, color));
                            continue;
                        }
                    }
                    firstLine++;
                    newArr.Add(srcLine);
                }
                arr = newArr;

                // Make sure highlights don't overlap each other or go past the end of the block
                highlight.Add((arr.Count, 0, ""));
                for (var i = 0; i < highlight.Count - 1; i++)
                {
                    var (currFirst, currNum, currCol) = highlight[i];
                    var (nextFirst, _, _) = highlight[i + 1];
                    var lastLine = currFirst + currNum - 1;
                    if (lastLine >= nextFirst) highlight[i] = (currFirst, nextFirst - currFirst, currCol);
                }
                highlight.RemoveAll(x => x.numLines <= 0);
            }

            arr = FixCodeIndentation(arr);

            var highlights = string.Join("", highlight.Select(
                h => $"<div class=\"line-highlight\" style=\"top: {h.firstLine}em; height: {h.numLines}em; background: {h.color};\"></div>")
            );
            var plain = new UnprocessablePlainTextNode(String.Join("\n", arr));
            var cls = string.IsNullOrWhiteSpace(lang) ? "" : $" class=\"lang-{lang}\"";
            var before = $"<pre{cls}><code>{highlights}";
            var after = "</code></pre>";
            return new HtmlNode(before, plain, after);
        }

        public static List<string> FixCodeIndentation(List<string> arr)
        {
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
            return arr.Select(a => a.Substring(Math.Min(longestWhitespace, a.Length))).ToList();
        }
    }
}
