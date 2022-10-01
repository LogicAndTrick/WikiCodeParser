using System.Collections.Generic;
using System.Text.RegularExpressions;
using LogicAndTrick.WikiCodeParser.Nodes;

namespace LogicAndTrick.WikiCodeParser.Tags
{
    public class YoutubeTag : Tag
    {
        public YoutubeTag()
        {
            Token = "youtube";
            Element = "div";
            MainOption = "id";
            Options = new[] {"id"};
        }

        public override INode FormatResult(Parser parser, ParseData data, State state, string scope, Dictionary<string, string> options, string text)
        {
            var id = text;
            if (options.ContainsKey("id")) id = options["id"];

            var classes = new List<string> {"embedded", "video"};
            if (ElementClass != null) classes.Add(ElementClass);

            var captionNode = new HtmlNode("", new PlainTextNode(""), "") { PlainBefore = "[YouTube video] ", PlainAfter = "\n" };

            var before = $"<div class=\"{string.Join(" ", classes)}\">" +
                         $" <div class=\"caption-panel\">" +
                         $"  <div class=\"video-container caption-body\">" +
                         $"   <div class=\"video-content\">" +
                         $"    <div class=\"uninitialised\" data-youtube-id=\"{id}\" style=\"background-image: url('https://i.ytimg.com/vi/{id}/hqdefault.jpg');\"></div>" +
                         $"   </div>" +
                         $"  </div>";
            var after = $"</div></div>";
            return new HtmlNode(before, captionNode, after)
            {
                IsBlockNode = true
            };
        }

        public override bool Validate(Dictionary<string, string> options, string text)
        {
            var url = text;
            if (options.ContainsKey("id")) url = options["id"];
            return Regex.IsMatch(url, @"^[a-zA-Z0-9_-]{6,11}$", RegexOptions.IgnoreCase);
        }
    }
}
