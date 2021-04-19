using WikiCodeParser.Models;
using WikiCodeParser.Nodes;

namespace WikiCodeParser.Tags
{
    public class WikiArchiveTag : Tag
    {
        public WikiArchiveTag()
        {
            Token = null;
            Element = "";
        }

        public override bool Matches(State state, string token)
        {
            var peekTag = state.Peek(9);
            var pt = state.PeekTo("]");
            return peekTag == "[archive:" && pt != null && pt.Length > 9 && !pt.Contains("\n");
        }

        public override INode Parse(Parser parser, ParseData data, State state, string scope)
        {
            var index = state.Index;
            if (state.Next() != '[')
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

            var credit = new WikiRevisionCredit
            {
                Type = WikiRevisionCredit.TypeArchive
            };

            var sections = str.Split('|');
            foreach (var section in sections)
            {
                var spl = section.Split(new[] {':'}, 2);
                var key = spl[0];
                var val = spl.Length > 1 ? spl[1] : "";
                switch (key) {
                    case "archive":
                        credit.Name = val;
                        break;
                    case "description":
                        credit.Description = val;
                        break;
                    case "url":
                        credit.Url = val;
                        break;
                    case "wayback":
                        credit.WaybackUrl = val;
                        break;
                    case "full":
                        credit.Type = WikiRevisionCredit.TypeFull;
                        break;
                }
            }
            if (credit.WaybackUrl != null && credit.Url != null && !credit.WaybackUrl.StartsWith("http://") && !credit.WaybackUrl.StartsWith("https://") && int.TryParse(credit.WaybackUrl, out _))
            {
                credit.WaybackUrl = $"https://web.archive.org/web/{credit.WaybackUrl}/{credit.Url}";
            }

            state.SkipWhitespace();
            return new MetadataNode("WikiCredit", credit);
        }
    }
}