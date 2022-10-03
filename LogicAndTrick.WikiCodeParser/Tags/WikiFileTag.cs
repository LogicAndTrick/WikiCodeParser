using System.Text.RegularExpressions;
using LogicAndTrick.WikiCodeParser.Models;
using LogicAndTrick.WikiCodeParser.Nodes;

namespace LogicAndTrick.WikiCodeParser.Tags
{
    public class WikiFileTag : Tag
    {
        private static string GetTag(State state)
        {
            var peekTag = state.Peek(6);
            var pt = state.PeekTo("]");
            if (peekTag == "[file:" && pt?.Length > 6 && !pt.Contains("\n")) return "file";
            return null;
        }

        public override bool Matches(State state, string token, TagParseContext context)
        {
            var tag = GetTag(state);
            return tag != null;
        }

        public override INode Parse(Parser parser, ParseData data, State state, string scope, TagParseContext context)
        {
            var index = state.Index;

            var tag = GetTag(state);
            if (state.ScanTo(":") != $"[{tag}" || state.Next() != ':')
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

            var match = Regex.Match(str, @"^([^#\]]+?)(?:\|([^\]]*?))?$", RegexOptions.IgnoreCase);

            if (!match.Success)
            {
                state.Seek(index, true);
                return null;
            }

            var page = match.Groups[1].Value;
            var text = match.Groups[2].Value.Length > 0 ? match.Groups[2].Value : page;
            var slug = WikiRevision.CreateSlug(page);
            var url = HtmlHelper.AttributeEncode($"https://twhl.info/wiki/embed/{slug}");
            var infoUrl = HtmlHelper.AttributeEncode($"https://twhl.info/wiki/embed-info/{slug}");

            var before = $"<span class=\"embedded-inline download\" data-info=\"{infoUrl}\"><a href=\"{url}\"><span class=\"fa fa-download\"></span> ";
            var after = "</a></span>";

            var content = new NodeCollection();
            content.Nodes.Add(new MetadataNode("WikiUpload", page));
            content.Nodes.Add(new PlainTextNode(text));

            return new HtmlNode(before, content, after);
        }
    }
}
