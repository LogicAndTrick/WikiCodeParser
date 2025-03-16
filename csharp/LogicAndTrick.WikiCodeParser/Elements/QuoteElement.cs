using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using LogicAndTrick.WikiCodeParser.Nodes;

namespace LogicAndTrick.WikiCodeParser.Elements
{
    public class QuoteElement : Element
    {
        private static readonly Regex OpenQuote = new Regex(@"^\[quote(?:(?: name)?=([^]]*))?\]", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private const int CloseQuoteLength = 8;

        public override bool Matches(Lines lines)
        {
            var value = lines.Value().Trim();
            return value.Length > 6 && value.StartsWith("[quote") && OpenQuote.IsMatch(value);
        }

        public override INode Consume(Parser parser, ParseData data, Lines lines, string scope)
        {
            var current = lines.Current();
            var arr = new List<string>();

            var line = lines.Value().Trim();
            var res = OpenQuote.Match(line);
            if (!res.Success)
            {
                lines.SetCurrent(current);
                return null;
            }

            var text = BalanceQuotes(lines, out var author, out var postfix);
            if (text == null)
            {
                lines.SetCurrent(current);
                return null;
            }

            var before = "<blockquote>";
            var plainBefore = "[quote]\n";
            if (!string.IsNullOrWhiteSpace(author))
            {
                before += "<strong class=\"quote-name\">" + author + " said:</strong><br/>";
                plainBefore = author + " said: " + plainBefore;
            }

            var node = new HtmlNode(before, parser.ParseElements(data, text, scope), "</blockquote>")
            {
                PlainBefore = plainBefore,
                PlainAfter = "\n[/quote]",
                IsBlockNode = true,
            };
            if (!string.IsNullOrWhiteSpace(postfix))
            {
                return new NodeCollection(node, parser.ParseTags(data, postfix, scope, TagParseContext.Inline));
            }
            return node;
        }

        public static string BalanceQuotes(Lines lines, out string name, out string postfix)
        {
            name = null;
            postfix = null;

            var line = lines.Value().TrimStart();
            var openMat = OpenQuote.Match(line);
            if (!openMat.Success) return null;
            if (openMat.Groups[1].Success) name = openMat.Groups[1].Value;

            line = line.Substring(openMat.Length);
            var arr = new List<string>();
            var currentLevel = 1;
            do
            {
                var idx = 0;
                do
                {
                    openMat = OpenQuote.Match(line, idx);
                    var openMatIdx = openMat.Success ? openMat.Index : -1;
                    var closeMatIdx = line.IndexOf("[/quote]", idx, StringComparison.InvariantCultureIgnoreCase);

                    if (openMatIdx >= 0 && (closeMatIdx < 0 || closeMatIdx > openMatIdx))
                    {
                        // Open quote
                        currentLevel++;
                        idx = openMat.Index + openMat.Length;
                    }
                    else if (closeMatIdx >= 0)
                    {
                        // Close quote
                        currentLevel--;
                        if (currentLevel == 0)
                        {
                            if (line.Length > closeMatIdx + CloseQuoteLength) postfix = line.Substring(closeMatIdx + CloseQuoteLength);
                            arr.Add(line.Substring(0, closeMatIdx));
                            return string.Join("\n", arr);
                        }
                        idx = closeMatIdx + CloseQuoteLength;
                    }
                    else
                    {
                        arr.Add(line);
                        break;
                    }
                } while (true);

                if (lines.Next()) line = lines.Value();
                else break;
            } while (true);

            return null;
        }
    }
}
