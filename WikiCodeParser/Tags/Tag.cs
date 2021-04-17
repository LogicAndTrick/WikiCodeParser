using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using WikiCodeParser.Nodes;

namespace WikiCodeParser.Tags
{
    public abstract class Tag
    {
        public abstract string Token { get; }
        public abstract string Element { get; }
        public virtual string ElementClass => null;
        public virtual string MainOption => null;
        public virtual bool AllOptionsInMain => false;
        public virtual bool IsBlock => false;
        public virtual bool IsNested => false;

        public List<string> Scopes { get; set; }
        public int Priority { get; set; } = 0;

        public virtual bool InScope(string scope) => string.IsNullOrWhiteSpace(scope) ||
                                                     Scopes.Contains(scope, StringComparer.InvariantCultureIgnoreCase);

        public virtual bool Matches(State state, string token) => token.ToLower() == Token;

        public virtual INode Parse(Parser parser, State state, string scope)
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
                    var matches = Regex.Matches(optionsString, @"(?=\s|^)\s*([^ ]+?)=([^\s]*)\b(?!=)",
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
                    if (tok == Token) stack++;
                    if (tok == "/" + Token && state.Peek(tokenLength + 3) == "[/" + Token + "]") stack--;
                    if (stack == 0)
                    {
                        state.Seek(Token.Length + 3, false);
                        if (!this.Validate(options, text)) break;
                        return FormatResult(parser, state, scope, options, text);
                    }

                    text += state.Next();
                }

                state.Seek(index, true);
                return null;
            }
            else
            {
                var text = state.ScanTo("[/" + Token + "]");
                if (state.Peek(tokenLength + 3).ToLower() == "[/" + Token + "]" && this.Validate(options, text))
                {
                    state.Seek(Token.Length + 3, false);
                    return FormatResult(parser, state, scope, options, text);
                }
                else
                {
                    state.Seek(index, true);
                    return null;
                }
            }
        }

        public virtual bool Validate(Dictionary<string, string> options, string text) => true;

        public INode FormatResult(Parser parser, State state, string scope, Dictionary<string, string> options, string text)
        {
            var before = "<" + Element;
            if (ElementClass != null) before += " class=\"" + ElementClass + '"';
            before += '>';
            var after = "</" + Element + '>';
            var content = parser.ParseTags(text, scope, IsBlock ? "block" : "inline");
            return new HtmlNode(before, content, after);
        }
    }
}