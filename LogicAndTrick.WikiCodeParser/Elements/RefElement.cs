using System.Collections.Generic;
using System.Text.RegularExpressions;
using LogicAndTrick.WikiCodeParser.Nodes;

namespace LogicAndTrick.WikiCodeParser.Elements
{
    public class RefElement : Element
    {
        public override bool Matches(Lines lines)
        {
            var value = lines.Value().Trim();
            return value.Length > 4 && value.StartsWith("[ref=") && Regex.IsMatch(value, @"\[ref=[a-z]+\]", RegexOptions.IgnoreCase);
        }

        public override INode Consume(Parser parser, ParseData data, Lines lines, string scope)
        {
            var current = lines.Current();
            var arr = new List<string>();

            var line = lines.Value().Trim();
            var res = Regex.Match(line, @"\[ref=([a-z ]+)\]", RegexOptions.IgnoreCase);
            if (!res.Success)
            {
                lines.SetCurrent(current);
                return null;
            }

            line = line.Substring(res.Value.Length);

            var name = res.Groups[1].Value;

            if (line.EndsWith("[/ref]"))
            {
                lines.Next();
                arr.Add(line.Substring(0, line.Length - 6));
            }
            else
            {
                if (line.Length > 0) arr.Add(line);
                var found = false;
                while (lines.Next())
                {
                    var value = lines.Value().TrimEnd();
                    if (value.EndsWith("[/ref]"))
                    {
                        var lastLine = value.Substring(0, value.Length - 6);
                        arr.Add(lastLine);
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
            }

            // Store the ref node
            var node = parser.ParseElements(data, string.Join("\n", arr).Trim(), scope);
            data.Set($"Ref::{name}", node);

            // Return nothing
            return PlainTextNode.Empty;
        }
    }
}
