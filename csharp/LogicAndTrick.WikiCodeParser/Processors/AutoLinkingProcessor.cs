using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using LogicAndTrick.WikiCodeParser.Nodes;

namespace LogicAndTrick.WikiCodeParser.Processors
{
    public class AutoLinkingProcessor : INodeProcessor
    {
        public int Priority { get; set; } = 11;

        public bool ShouldProcess(INode node, string scope)
        {
            // handle <a> parent nodes so we skip processing of everything under them
            if (node is HtmlNode html && html.HtmlBefore.Contains("<a ")) return true;
            return node is PlainTextNode ptn
                   && (ptn.Text.ToLower().Contains("http") || ptn.Text.Contains("@"));
        }

        private readonly static Regex UrlMatch = new Regex(@"(?<![^\s""(])(?<url>https?://[^\][""\s]+)(?=[.,!?""]{0,5}(?!\S))", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private readonly static Regex EmailMatch = new Regex(@"(?<![^\s""(])(?<email>[^\][""\s@]+@[^\][""\s@.]+(?:\.[^\][""\s@.]+)+)(?=[.,!?""]{0,5}(?!\S))", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        
        private static readonly HashSet<char> AllowedStartChars = new HashSet<char> { ' ', '\n', '\r', '\t', '(', '"' };
        private static readonly HashSet<char> AllowedEndChars = new HashSet<char> { ' ', '\n', '\r', '\t', ')', '"', '.', ',', '!', '?' };

        public IEnumerable<INode> Process(Parser parser, ParseData data, INode parentNode, INode node, string scope)
        {
            if (!(node is PlainTextNode))
            {
                yield return node;
                yield break;
            }

            var text = ((PlainTextNode) node).Text;

            var urlMatches = UrlMatch.Matches(text);
            var emailMatches = EmailMatch.Matches(text);
            var start = 0;
            foreach (var match in urlMatches.OfType<Match>().Union(emailMatches.OfType<Match>()).OrderBy(x => x.Index))
            {
                var idx = match.Index;
                var len = match.Length;
                if (idx < start) continue;

                // skip matches starting on the first letter if the previous sibling doesn't end in whitespace or one of the allowed punctuation chars
                if (idx == 0 && !node.PreviousNodeEndsWith(parentNode, AllowedStartChars)) continue;

                // walk back any punctuation or brackets if they are balanced
                while (len > 0 && ShouldWalkBack(text, idx, len)) len--;

                if (len < 5) continue; // urls and emails can't be this short. something is wrong, so skip it.

                // skip matches ending on the last letter if the next sibling doesn't start in whitespace or one of the allowed punctuation chars
                if (idx + len == text.Length && !node.NextNodeStartsWith(parentNode, AllowedEndChars)) continue;
                
                var matchingText = text.Substring(idx, len);
                if (match.Groups["url"].Success && !HtmlHelper.ValidateUrl(matchingText)) continue;

                // return the prefix text
                if (idx > start) yield return new PlainTextNode(text.Substring(start, idx - start));

                if (match.Groups["url"].Success)
                {
                    yield return new HtmlNode($"<a href=\"{HtmlHelper.AttributeEncode(HtmlHelper.SanitiseUrl(matchingText))}\">", new UnprocessablePlainTextNode(matchingText), "</a>");
                }
                else if (match.Groups["email"].Success)
                {
                    yield return new HtmlNode($"<a href=\"mailto:{HtmlHelper.AttributeEncode(matchingText)}\">", new UnprocessablePlainTextNode(matchingText), "</a>");
                }
                start = idx + len;
            }
            
            // return the remaining text
            if (start < text.Length) yield return new PlainTextNode(text.Substring(start));
        }

        private static readonly HashSet<char> Punctuation = new HashSet<char> { '.', '!', '?', ',' };
        private static readonly HashSet<char> UrlFragments = new HashSet<char> { '/', '#', '?', '&', ':' };

        private static bool ShouldWalkBack(string text, int idx, int len)
        {
            var c = text[idx + len - 1]; // last char of the match
            if (Punctuation.Contains(c)) return true; // english punctuation, unlikely a url ends with these

            // if we get a close paren, then try to balance them
            if (c == ')')
            {
                var lvl = 0;
                do {
                    c = text[idx + len - 1];
                    if (c == ')') lvl++;
                    else if (c == '(') lvl--;
                    else if (UrlFragments.Contains(c)) break; // url/route fragment, assume it can't be balanced
                    len--;
                } while (len > 0 && lvl != 0);
                if (lvl > 0) return true;
            }
            return false;
        }
    }
}
