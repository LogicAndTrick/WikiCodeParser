using System;
using System.Collections.Generic;
using System.Linq;
using LogicAndTrick.WikiCodeParser.Nodes;

namespace LogicAndTrick.WikiCodeParser.Processors
{
    public class MarkdownTextProcessor : INodeProcessor
    {
        public int Priority { get; set; } = 10;

        private static readonly char[] Tokens = "`*/_~".ToCharArray();
        private static readonly string[] OpenTags = { "<code>", "<strong>", "<em>", "<span class=\"underline\">", "<span class=\"strikethrough\">" };
        private static readonly string[] CloseTags = { "</code>", "</strong>", "</em>", "</span>", "</span>" };
        private static readonly char[] StartBreakChars = "!^()+=[]{}\"'<>?,. \t\r\n".ToCharArray();
        private static readonly char[] ExtraEndBreakChars = ":;".ToCharArray();

        public bool ShouldProcess(INode node, string scope)
        {
            return node is PlainTextNode ptn && ptn.Text.IndexOfAny(Tokens) >= 0;
        }

        private static int GetTokenIndex(char c) => Array.IndexOf(Tokens, c);
        private static bool IsStartBreakChar(char c) => StartBreakChars.Contains(c);
        private static bool IsEndBreakChar(char c) => StartBreakChars.Contains(c) || ExtraEndBreakChars.Contains(c);

        public IEnumerable<INode> Process(Parser parser, ParseData data, INode node, string scope)
        {
            var text = ((PlainTextNode) node).Text;

            /*
             * Like everything else here, this isn't exactly markdown, but it's close.
             * _underline_
             * /italics/
             * *bold*
             * ~strikethrough~
             * `code`
             * Very simple rules: no nesting, no newlines, must start/end on a word boundary
             */
            
            // pre-condition: start of a line OR one of: !?^()+=[]{}"'<>,. OR whitespace
            // first and last character is NOT whitespace. everything else is fine except for newlines
            // post-condition: end of a line OR one of: !?^()+=[]{}"'<>,.:; OR whitespace

            var tracker = new int[Tokens.Length];
            void ResetTracker() { for (var j = 0; j < tracker.Length; j++) tracker[j] = -1; }
            ResetTracker();

            var plainStart = 0;
            int i;
            for (i = 0; i < text.Length; i++)
            {
                var c = text[i];

                // No newlines in tokens
                if (c == '\n' || c == '\r')
                {
                    ResetTracker();
                    continue;
                }

                var tokenIndex = GetTokenIndex(c);
                if (tokenIndex < 0) continue;

                // Check if we're in this token, and we can close it out
                if (tracker[tokenIndex] >= 0 && (i + 1 == text.Length || IsEndBreakChar(text[i + 1])) && !Char.IsWhiteSpace(text, i - 1))
                {
                    var start = tracker[tokenIndex];
                    var end = i;
                    if (plainStart < start) yield return new PlainTextNode(text.Substring(plainStart, start - plainStart));
                    yield return new HtmlNode(OpenTags[tokenIndex], new PlainTextNode(text.Substring(start + 1, end - start - 1)), CloseTags[tokenIndex]);
                    ResetTracker();
                    plainStart = i + 1;
                }

                // Check if we can open a new token
                else if ((i == 0 || IsStartBreakChar(text[i - 1])) && i + 1 < text.Length && !Char.IsWhiteSpace(text, i + 1))
                {
                    tracker[tokenIndex] = i;
                }
            }

            // Return the rest of the text as plain
            if (plainStart < text.Length) yield return new PlainTextNode(text.Substring(plainStart));
        }
    }
}