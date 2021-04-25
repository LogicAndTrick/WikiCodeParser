using System.Collections.Generic;
using System.Text.RegularExpressions;
using WikiCodeParser.Nodes;

namespace WikiCodeParser.Elements
{
    public class RefElement : Element
    {
        public override bool Matches(Lines lines)
        {
            var value = lines.Value().Trim();
            return value.Length > 4 && value.StartsWith("[ref=") && value.EndsWith("]") && Regex.IsMatch(value, @"\[ref=[a-z]+\]", RegexOptions.IgnoreCase);
        }

        public override INode Consume(Parser parser, ParseData data, Lines lines, string scope)
        {
            var current = lines.Current();

            var line = lines.Value().Trim();
            var res = Regex.Match(line, @"\[ref=([a-z ]+)\]", RegexOptions.IgnoreCase);
            if (!res.Success)
            {
                lines.SetCurrent(current);
                return null;
            }

            var name = res.Groups[1].Value;
            var arr = new List<string>();
            var found = false;

            while (lines.Next())
            {
                var value = lines.Value().TrimEnd();
                if (value == "[/ref]")
                {
                    found = true;
                    break;
                }
                else
                {
                    arr.Add(value);
                }
            }

            if (!found || arr.Count == 0)
            {
                lines.SetCurrent(current);
                return null;
            }

            // Trim blank lines from the start and end of the array
            for (var i = 0; i < 2; i++)
            {
                while (arr.Count > 0 && arr[0].Trim() == "") arr.RemoveAt(0);
                arr.Reverse();
            }

            // Store the ref node
            var node = parser.ParseElements(data, string.Join("\n", arr), scope);
            data.Set($"Ref::{name}", node);

            // Return nothing
            return PlainTextNode.Empty;
        }
    }
}
