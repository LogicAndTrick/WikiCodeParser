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
        private static bool IsEndBreakChar(char c) => StartBreakChars.Contains(c) || ExtraEndBreakChars.Contains(c) || Tokens.Contains(c);
        
        private static INode ParseToken(int[] tracker, string text, int position, out int endPosition)
        {
            endPosition = -1;
            var token = text[position];
            var tokenIndex = GetTokenIndex(token);

            // Make sure we're not already in this token
            if (tracker[tokenIndex] != 0) return null;

            var endToken = text.IndexOf(token, position + 1);
            if (endToken <= position + 1) return null;
            if (text.IndexOf('\n', position + 1, endToken - position - 1) >= 0) return null; // no newlines
            
            // Make sure we can close this token
            var valid = (endToken + 1 == text.Length || IsEndBreakChar(text[endToken + 1])) // end of string or before an end breaker
                        && !Char.IsWhiteSpace(text, endToken - 1); // not whitespace previous
            if (!valid) return null;

            var str = text.Substring(position + 1, endToken - position - 1);

            tracker[tokenIndex] = 1;

            // code tokens cannot be nested
            INode contents;
            if (token == '`')
            {
                contents = new UnprocessablePlainTextNode(str);
            }
            else
            {
                var toks = ParseTokens(tracker, str).ToList();
                contents = toks.Count == 1 ? toks[0] : new NodeCollection(toks);
            }

            tracker[tokenIndex] = 0;

            endPosition = endToken;

            return new HtmlNode(OpenTags[tokenIndex], contents, CloseTags[tokenIndex]);
        }

        private static IEnumerable<INode> ParseTokens(int[] tracker, string text)
        {
            var plainStart = 0;
            var index = 0;
            while (true)
            {
                var nextIndex = text.IndexOfAny(Tokens, index);
                if (nextIndex < 0) break;

                // Make sure we can start a new token
                var valid = (nextIndex == 0 || IsStartBreakChar(text[nextIndex - 1])) // start of string or after a start breaker
                            && nextIndex + 1 < text.Length // not end of string
                            && !Char.IsWhiteSpace(text, nextIndex + 1); // not whitespace next
                if (!valid)
                {
                    index = nextIndex + 1;
                    continue;
                }

                var parsed = ParseToken(tracker, text, nextIndex, out var endIndex);
                if (parsed == null)
                {
                    index = nextIndex + 1; // no match, skip this token
                }
                else
                {
                    if (plainStart < nextIndex) yield return new PlainTextNode(text.Substring(plainStart, nextIndex - plainStart));
                    yield return parsed;
                    index = plainStart = endIndex + 1;
                }
            }
            // Return the rest of the text as plain
            if (plainStart < text.Length) yield return new PlainTextNode(text.Substring(plainStart));
        }

        public IEnumerable<INode> Process(Parser parser, ParseData data, INode node, string scope)
        {
            var text = ((PlainTextNode) node).Text;

            var nextIndex = text.IndexOfAny(Tokens, 0);
            if (nextIndex < 0)
            {
                // Short circuit
                yield return node;
                yield break;
            }

            /*
             * Like everything else here, this isn't exactly markdown, but it's close.
             * _underline_
             * /italics/
             * *bold*
             * ~strikethrough~
             * `code`
             * Very simple rules: no newlines, must start/end on a word boundary, code tags cannot be nested
             */

            // pre-condition: start of a line OR one of: !?^()+=[]{}"'<>,. OR whitespace
            // first and last character is NOT whitespace. everything else is fine except for newlines
            // post-condition: end of a line OR one of: !?^()+=[]{}"'<>,.:; OR whitespace

            var tracker = new int[Tokens.Length];

            foreach (var token in ParseTokens(tracker, text))
            {
                yield return token;
            }
        }
    }
}