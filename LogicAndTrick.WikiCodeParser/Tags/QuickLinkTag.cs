using System.Collections.Generic;
using System.Text.RegularExpressions;
using LogicAndTrick.WikiCodeParser.Nodes;

namespace LogicAndTrick.WikiCodeParser.Tags
{
    public class QuickLinkTag : Tag
    {
        public QuickLinkTag()
        {
            Token = null;
            Element = "a";
            MainOption = "url";
            Options = new[] {"url"};
        }

        public override bool Matches(State state, string token)
        {
            var pt = state.PeekTo("]");
            if (string.IsNullOrEmpty(pt)) return false;

            pt = pt.Substring(1);
            return pt.Length > 0 && !pt.Contains("\n") && Regex.IsMatch(pt, @"^([a-z]{2,10}://[^\]]*?)(?:\|([^\]]*?))?", RegexOptions.IgnoreCase);
        }

        public override INode Parse(Parser parser, ParseData data, State state, string scope)
        {
            var index = state.Index;

            if (state.Next() != '[')
            {
                state.Seek(index, true);
                return null;
            }

            var str = state.ScanTo("]");
            if (state.Next() != ']')
            {
                state.Seek(index, true);
                return null;
            }

            var match = Regex.Match(str, @"^([a-z]{2,10}://[^\]]*?)(?:\|([^\]]*?))?$", RegexOptions.IgnoreCase);
            if (!match.Success)
            {
                state.Seek(index, true);
                return null;
            }

            var url = match.Groups[1].Value;
            var text = match.Groups[2].Length > 0 ? match.Groups[2].Value : url;
            var options = new Dictionary<string, string> {{"url", url}};
            if (!Validate(options, text))
            {
                state.Seek(index, true);
                return null;
            }

            url = System.Web.HttpUtility.HtmlAttributeEncode(url);
            var before = $"<{Element} href=\"{url}\">";
            var after = $"</{Element}>";

            var content = new PlainTextNode(text);
            return new HtmlNode(before, content, after)
            {
                PlainAfter = match.Groups[2].Length > 0 ? $" ({url})" : ""
            };
        }

        public override bool Validate(Dictionary<string, string> options, string text)
        {
            var url = text;
            if (options.ContainsKey("url")) url = options["url"];
            return !url.Contains("<script") && Regex.IsMatch(url, @"^([a-z]{2,10}://)?([^]""\n ]+?)", RegexOptions.IgnoreCase);
        }
    }
}