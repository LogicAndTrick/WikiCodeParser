using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using LogicAndTrick.WikiCodeParser.Nodes;

namespace LogicAndTrick.WikiCodeParser.Tags
{
    public class WikiYoutubeTag : LinkTag
    {
        public WikiYoutubeTag()
        {
            Token = null;
            Element = "div";
            MainOption = "id";
            Options = new[] {"id"};
        }

        public override bool Matches(State state, string token)
        {
            var peekTag = state.Peek(9);
            var pt = state.PeekTo("]");
            return peekTag == "[youtube:" && pt != null && pt.Length > 9 && !pt.Contains("\n");
        }

        public override INode Parse(Parser parser, ParseData data, State state, string scope)
        {
            var index = state.Index;
            if (state.ScanTo(":") != "[youtube" || state.Next() != ':')
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

            var regs = Regex.Match(str, @"^([^|\]]*?)(?:\|([^\]]*?))?$", RegexOptions.IgnoreCase);
            if (!regs.Success)
            {
                state.Seek(index, true);
                return null;
            }

            var id = regs.Groups[1].Value;
            var @params = regs.Groups[2].Value.Trim().Split('|');

            if (!ValidateID(id))
            {
                state.Seek(index, true);
                return null;
            }

            state.SkipWhitespace();

            string caption = null;
            var classes = new List<string> {"embedded", "video"};
            if (ElementClass != null) classes.Add(ElementClass);
            foreach (var p in @params)
            {
                var l = p.ToLower();
                if (IsClass(l)) classes.Add(l);
                else caption = p.Trim();
            }

            var captionNode = new HtmlNode(
                caption != null ? "<span class=\"caption\">" : "",
                new PlainTextNode(caption ?? ""),
                caption != null ? "</span>" : ""
            ) { PlainBefore = "[YouTube video] ", PlainAfter = "\n" };

            var before = $"<div class=\"{string.Join(" ", classes)}\">" +
                         $" <div class=\"caption-panel\">" +
                         $"  <div class=\"video-container caption-body\">" +
                         $"   <div class=\"video-content\">" +
                         $"    <div class=\"uninitialised\" data-youtube-id=\"{id}\" style=\"background-image: url('https://i.ytimg.com/vi/{id}/hqdefault.jpg');\"></div>" +
                         $"   </div>" +
                         $"  </div>";
            var after = $"</div></div>";
            return new HtmlNode(before, captionNode, after);
        }

        private bool ValidateID(string id)
        {
            return Regex.IsMatch(id, @"^[a-zA-Z0-9_-]{6,11}$", RegexOptions.IgnoreCase);
        }

        private static readonly string[] ValidClasses =
        {
            "large", "medium", "small", "left", "right", "center"
        };

        private bool IsClass(string param) => ValidClasses.Contains(param);
    }
}
