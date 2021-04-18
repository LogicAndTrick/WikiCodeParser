using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using WikiCodeParser.Models;
using WikiCodeParser.Nodes;

namespace WikiCodeParser.Tags
{
    public class WikiImageTag : LinkTag
    {
        public WikiImageTag()
        {
            Token = null;
            Element = "img";
        }

        private static readonly string[] Tags = {"img", "video", "audio"};

        private static string GetTag(State state)
        {
            foreach (var tag in Tags)
            {
                var peekTag = state.Peek(2 + tag.Length);
                var pt = state.PeekTo("]");
                if (peekTag == $"[{tag}:" && pt?.Length > 2 + tag.Length && !pt.Contains("\n")) return tag;
            }

            return null;
        }

        public override bool Matches(State state, string token)
        {
            var tag = GetTag(state);
            return tag != null;
        }

        public override INode Parse(Parser parser, State state, string scope)
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

            var match = Regex.Match(str, @"^([^|\]]*?)(?:\|([^\]]*?))?$", RegexOptions.IgnoreCase);

            if (!match.Success)
            {
                state.Seek(index, true);
                return null;
            }

            var content = new NodeCollection();

            var image = match.Groups[1].Value;
            var @params = match.Groups[2].Success ? match.Groups[2].Value.Trim().Split('|') : new string[0];
            var src = image;
            if (!image.Contains("/"))
            {
                image = System.Web.HttpUtility.HtmlDecode(image);
                content.Nodes.Add(new MetadataNode("WikiUpload", image));
                src = System.Web.HttpUtility.HtmlAttributeEncode($"https://twhl.info/wiki/embed/{WikiRevision.CreateSlug(image)}");
            }

            string url = null;
            string caption = null;
            var loop = false;

            var classes = new List<string> {"embedded", "image"};
            if (ElementClass != null) classes.Add(ElementClass);

            foreach (var p in @params)
            {
                var l = p.ToLower();
                if (IsClass(l)) classes.Add(l);
                else if (l == "loop") loop = true;
                else if (l.Length > 4 && l.Substring(0, 4) == "url:") url = p.Substring(4).Trim();
                else caption = p.Trim();
            }

            if (tag == "img" && url != null && ValidateUrl(url))
            {
                if (Regex.IsMatch(url, @"^[a-z]{2,10}://", RegexOptions.IgnoreCase))
                {
                    url = System.Web.HttpUtility.HtmlDecode(url);
                    content.Nodes.Add(new MetadataNode("WikiLink", url));
                    url = System.Web.HttpUtility.HtmlAttributeEncode($"https://twhl.info/wiki/page/{WikiRevision.CreateSlug(url)}");
                }
            }
            else
            {
                url = "";
            }

            var el = "span";

            // Non-inline images should eat any whitespace after them
            if (!classes.Contains("inline"))
            {
                state.SkipWhitespace();
                el = "div";
            }

            var embed = GetEmbedObject(tag, src, caption, loop);
            if (embed != null) content.Nodes.Add(embed);
            if (caption != null) content.Nodes.Add(new HtmlNode("<span class=\"caption\">", new PlainTextNode(caption), "</span>") { PlainAfter = "\n" });

            var before = $" <{el} class=\"{string.Join(" ", classes)}\"" + (caption?.Length > 0 ? $" title=\"{caption}\"" : "") + ">"
                         + (url.Length > 0 ? "<a href=\"" + System.Web.HttpUtility.HtmlAttributeEncode(url) + "\">" : "")
                         + "<span class=\"caption-panel\">";
            var after = "</span>"
                        + (url.Length > 0 ? "</a>" : "")
                        + $"</{el}> ";

            return new HtmlNode(before, content, after);
        }

        private INode GetEmbedObject(string tag, string url, string caption, bool loop)
        {
            url = System.Web.HttpUtility.HtmlAttributeEncode(url);
            switch (tag)
            {
                case "img":
                    caption = caption ?? "User posted image";
                    var cap = System.Web.HttpUtility.HtmlAttributeEncode(caption);
                    return new HtmlNode($"<img class=\"caption-body\" src=\"{url}\" alt=\"{cap}\" />", PlainTextNode.Empty, "")
                    {
                        PlainBefore = "[Image] "
                    };
                case "video":
                case "audio":
                    var auto = "";
                    if (loop) auto = "autoplay loop muted";
                    return new HtmlNode($"<{tag} class=\"caption-body\" src=\"{url}\" playsinline controls {auto}>Your browser doesn't support embedded {tag}.</{tag}>", PlainTextNode.Empty, "")
                    {
                        PlainBefore = tag.Substring(0, 1).ToUpper() + tag.Substring(1)
                    };
            }

            return null;
        }

        private bool ValidateUrl(string url)
        {
            return !url.Contains("<script");
        }

        private static readonly string[] ValidClasses = {"large", "medium", "small", "thumb", "left", "right", "center", "inline"};
        private static bool IsClass(string param) => ValidClasses.Contains(param);
    }
}