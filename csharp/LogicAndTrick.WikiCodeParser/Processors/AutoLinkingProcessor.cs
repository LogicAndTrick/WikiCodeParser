using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using LogicAndTrick.WikiCodeParser.Nodes;

namespace LogicAndTrick.WikiCodeParser.Processors
{
    public class AutoLinkingProcessor : INodeProcessor
    {
        public int Priority { get; set; } = 9;

        public bool ShouldProcess(INode node, string scope)
        {
            return node is PlainTextNode ptn
                   && (ptn.Text.ToLower().Contains("http") || ptn.Text.Contains("@"));
        }

        private readonly static Regex UrlMatch = new Regex(@"(?<!\S)(?<url>https?://[^\][""\s]+)(?!\S)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private readonly static Regex EmailMatch = new Regex(@"(?<!\S)(?<email>[^\][""\s@]+@[^\][""\s@.]+(?:\.[^\][""\s@.]+)+)(?=\.?(?!\S))", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public IEnumerable<INode> Process(Parser parser, ParseData data, INode node, string scope)
        {
            var text = ((PlainTextNode) node).Text;

            var urlMatches = UrlMatch.Matches(text);
            var emailMatches = EmailMatch.Matches(text);
            var start = 0;
            foreach (var match in urlMatches.OfType<Match>().Union(emailMatches.OfType<Match>()).OrderBy(x => x.Index))
            {
                if (match.Index < start) continue;
                if (match.Index > start) yield return new PlainTextNode(text.Substring(start, match.Index - start));
                if (match.Groups["url"].Success)
                {
                    var url = match.Groups["url"].Value;
                    yield return new HtmlNode($"<a href=\"{HtmlHelper.AttributeEncode(url)}\">", new PlainTextNode(url), "</a>");
                }
                else if (match.Groups["email"].Success)
                {
                    var email = match.Groups["email"].Value;
                    yield return new HtmlNode($"<a href=\"mailto:{HtmlHelper.AttributeEncode(email)}\">", new PlainTextNode(email), "</a>");
                }
                start = match.Index + match.Length;
            }
            if (start < text.Length) yield return new PlainTextNode(text.Substring(start));
        }
    }
}
