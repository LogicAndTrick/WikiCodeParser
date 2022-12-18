using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using LogicAndTrick.WikiCodeParser.Nodes;

namespace LogicAndTrick.WikiCodeParser.Tags
{
    public class Tag
    {
        public string Token { get; set; }
        public string Element { get; set; }
        public string ElementClass { get; set; }
        public string MainOption { get; set; }
        public string[] Options { get; set; }
        public bool AllOptionsInMain { get; set; }
        public bool IsBlock { get; set; }
        public bool IsNested { get; set; }

        public List<string> Scopes { get; set; }
        public int Priority { get; set; } = 0;

        protected TagParseContext TagContext => IsBlock ? TagParseContext.Block : TagParseContext.Inline;

        protected Tag()
        {
            Options = Array.Empty<string>();
            Scopes = new List<string>();
        }

        public Tag(string token, string element, string elementClass = null) : this()
        {
            Token = token;
            Element = element;
            ElementClass = elementClass;
        }

        public virtual bool InScope(string scope) => string.IsNullOrWhiteSpace(scope) || Scopes.Contains(scope, StringComparer.InvariantCultureIgnoreCase);

        public virtual bool Matches(State state, string token, TagParseContext context) => token?.ToLower() == Token && (context == TagParseContext.Block || !IsBlock);

        public virtual INode Parse(Parser parser, ParseData data, State state, string scope, TagParseContext context)
        {
            var index = state.Index;
            var tokenLength = Token.Length;

            state.Seek(tokenLength + 1, false);
            var optionsString = state.ScanTo("]").Trim();
            if (state.Next() != ']')
            {
                state.Seek(index, true);
                return null;
            }

            var options = new Dictionary<string, string>();
            if (optionsString.Length > 0)
            {
                if (optionsString[0] == '=' && AllOptionsInMain && MainOption != null)
                {
                    options[MainOption] = optionsString.Substring(1);
                }
                else
                {
                    if (optionsString[0] == '=') optionsString = MainOption + optionsString;
                    var matches = Regex.Matches(optionsString, @"(?=\s|^)\s*([^ ]+?)=([^\s]*)(?=\s|$)(?!=)",
                        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Multiline);
                    foreach (Match m in matches)
                    {
                        var name = m.Groups[1].Value.Trim();
                        var value = m.Groups[2].Value.Trim();
                        options[name] = value;
                    }
                }
            }

            if (IsNested)
            {
                var stack = 1;
                var text = "";
                while (!state.Done)
                {
                    text += state.ScanTo("[");
                    var tok = state.GetToken();
                    if (string.Equals(tok, Token, StringComparison.InvariantCultureIgnoreCase)) stack++;
                    if (string.Equals(tok, "/" + Token, StringComparison.InvariantCultureIgnoreCase) && string.Equals(state.Peek(tokenLength + 3), "[/" + Token + "]", StringComparison.InvariantCultureIgnoreCase)) stack--;
                    if (stack == 0)
                    {
                        state.Seek(Token.Length + 3, false);
                        if (!Validate(options, text)) break;
                        return FormatResult(parser, data, state, scope, options, text);
                    }

                    text += state.Next();
                }

                state.Seek(index, true);
                return null;
            }
            else
            {
                var text = state.ScanTo("[/" + Token + "]", StringComparison.InvariantCultureIgnoreCase);
                if (string.Equals(state.Peek(tokenLength + 3), "[/" + Token + "]", StringComparison.InvariantCultureIgnoreCase) && Validate(options, text))
                {
                    state.Seek(Token.Length + 3, false);
                    return FormatResult(parser, data, state, scope, options, text);
                }
                else
                {
                    state.Seek(index, true);
                    return null;
                }
            }
        }

        public virtual bool Validate(Dictionary<string, string> options, string text) => true;

        public virtual INode FormatResult(Parser parser, ParseData data, State state, string scope, Dictionary<string, string> options, string text)
        {
            var before = "<" + Element;
            if (ElementClass != null) before += " class=\"" + ElementClass + '"';
            before += '>';
            var after = "</" + Element + '>';
            var content = parser.ParseTags(data, text, scope, TagContext);
            return new HtmlNode(before, content, after)
            {
                IsBlockNode = IsBlock
            };
        }
    }
}