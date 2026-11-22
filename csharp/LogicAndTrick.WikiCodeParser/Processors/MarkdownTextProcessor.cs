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

        private static INode ParseToken(int[] tracker, State state)
        {
            var tokStart = state.SourcePosition;
            var index = state.Index;
            var token = state.Next();
            var tokenIndex = GetTokenIndex(token);

            // Make sure we're not already in this token
            if (tracker[tokenIndex] != 0)
            {
                state.Seek(index, true);
                return null;
            }

            var strStart = state.SourcePosition;
            var str = state.ScanTo(token.ToString());

            // the contents cannot be empty or contain newlines, and the next char must be the token
            if (state.Next() != token || str.Length == 0 || str.IndexOf('\n') >= 0)
            {
                state.Seek(index, true);
                return null;
            }

            // Make sure we can close this token
            var valid = (state.Done || IsEndBreakChar(state.Peek(1)[0])) // end of string or before an end breaker
                        && Util.TrimEnd(str).Length == str.Length; // not whitespace previous
            if (!valid)
            {
                state.Seek(index, true);
                return null;
            }

            tracker[tokenIndex] = 1;

            // code tokens cannot be nested
            INode contents;
            if (token == '`')
            {
                contents = new UnprocessablePlainTextNode(strStart, str);
            }
            else
            {
                var toks = ParseTokens(strStart.Line, strStart.Column, tracker, str).ToList();
                contents = toks.Count == 1 ? toks[0] : new NodeCollection(toks);
            }

            tracker[tokenIndex] = 0;

            return new HtmlNode(tokStart, OpenTags[tokenIndex], contents, CloseTags[tokenIndex])
            {
                PlainBefore = token.ToString(),
                PlainAfter = token.ToString()
            };
        }

        private static IEnumerable<INode> ParseTokens(int line, int column, int[] tracker, string text)
        {
            var state = new State(line, column, text);
            var plainPos = state.SourcePosition;
            var plain = "";
            var advanceToken = false;
            while (!state.Done)
            {
                // always advance one token except at the start, or after we find a valid token
                if (advanceToken) plain += state.Next();
                advanceToken = true;

                plain += state.ScanToAny(Tokens);
                if (state.Done) break;

                // Make sure we can start a new token
                var valid = (state.Index == 0 || IsStartBreakChar(state.Text[state.Index - 1])) // start of string or after a start breaker
                            && state.Index + 1 < text.Length // not end of string
                            && Util.Trim(state.Text[state.Index + 1].ToString()) != ""; // not whitespace next
                if (!valid) continue;

                // Try to parse a token
                var parsed = ParseToken(tracker, state);
                if (parsed == null) continue;

                if (plain.Length > 0) yield return new PlainTextNode(plainPos, plain);
                yield return parsed;

                advanceToken = false;
                plain = "";
                plainPos = state.SourcePosition;
            }
            // Return the rest of the text as plain
            plain += state.ScanToEnd();
            if (plain.Length > 0) yield return new PlainTextNode(plainPos, plain);
        }

        public IEnumerable<INode> Process(Parser parser, ParseData data, INode parentNode, INode node, string scope)
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

            foreach (var token in ParseTokens(node.Line, node.Column, tracker, text))
            {
                yield return token;
            }
        }
    }
}