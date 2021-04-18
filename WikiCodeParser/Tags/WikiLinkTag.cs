using System.Collections.Generic;
using System.Text.RegularExpressions;
using WikiCodeParser.Models;
using WikiCodeParser.Nodes;

namespace WikiCodeParser.Tags
{
    public class WikiLinkTag : LinkTag
    {
        public WikiLinkTag()
        {
            Token = null;
        }

        public override bool Matches(State state, string token)
        {
            var pt = state.PeekTo("]]");
            return pt?.Length > 1 && pt[1] == '[' && !pt.Contains("\n")
                   && Regex.IsMatch(pt.Substring(2), @"([^\]]*?)(?:\|([^\]]*?))?", RegexOptions.IgnoreCase);
        }

        public override INode Parse(Parser parser, State state, string scope)
        {
            var index = state.Index;

            if (state.Next() != '[' || state.Next() != '[')
            {
                state.Seek(index, true);
                return null;
            }

            var str = state.ScanTo("]]");

            if (state.Next() != ']' || state.Next() != ']')
            {
                state.Seek(index, true);
                return null;
            }

            var match = Regex.Match(str, @"^([^\]]+?)(?:\|([^\]]*?))?$", RegexOptions.IgnoreCase);
            if (!match.Success)
            {
                state.Seek(index, true);
                return null;
            }

            var page = System.Web.HttpUtility.HtmlDecode(match.Groups[1].Value);
            var text = match.Groups[2].Success ? match.Groups[2].Value : page;
            
            var url = System.Web.HttpUtility.HtmlAttributeEncode($"https://twhl.info/wiki/page/{WikiRevision.CreateSlug(page)}");
            var before = $"<a href=\"{url}\">";
            var after = "</a>";
            
            var content = new NodeCollection();
            content.Nodes.Add(new MetadataNode("WikiLink", page));
            content.Nodes.Add(new PlainTextNode(text));

            return new HtmlNode(before, content, after);
        }
    }
}