using System.Collections.Generic;
using WikiCodeParser.Nodes;

namespace WikiCodeParser.Elements
{
    public class MdPanelElement : Element
    {
        public override bool Matches(Lines lines)
        {
            return lines.Value().StartsWith("~~~");
        }

        public override INode Consume(Parser parser, ParseData data, Lines lines, string scope)
        {
            var current = lines.Current();

            var meta = lines.Value().Substring(3);
            var title = "";

            var found = false;
            var arr = new List<string>();
            while (lines.Next())
            {
                var value = lines.Value().TrimEnd();
                if (value == "~~~")
                {
                    found = true;
                    break;
                }

                if (value.Length > 1 && value[0] == ':') title = value.Substring(1).Trim();
                else arr.Add(value);
            }

            if (!found)
            {
                lines.SetCurrent(current);
                return null;
            }

            string cls;
            if (meta == "message") cls = "card-success";
            else if (meta == "info") cls = "card-info";
            else if (meta == "warning") cls = "card-warning";
            else if (meta == "error") cls = "card-danger";
            else cls = "card-default";

            var before = $"<div class=\"embed-panel card {cls}\">" +
                         (title != "" ? $"<div class=\"card-header\">{System.Web.HttpUtility.HtmlEncode(title)}</div>" : "") +
                         "<div class=\"card-body\">";
            var content = parser.ParseElements(data, string.Join("\n", arr), scope);
            var after = "</div></div>";

            return new HtmlNode(before, content, after)
            {
                PlainBefore = title == "" ? "" : title + "\n" + new string('-', title.Length) + "\n"
            };
        }
    }
}
