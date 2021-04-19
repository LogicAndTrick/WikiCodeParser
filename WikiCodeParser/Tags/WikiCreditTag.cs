using WikiCodeParser.Models;
using WikiCodeParser.Nodes;

namespace WikiCodeParser.Tags
{
    public class WikiCreditTag : Tag
    {
        public WikiCreditTag()
        {
            Token = null;
            Element = "";
        }

        public override bool Matches(State state, string token)
        {
            var peekTag = state.Peek(8);
            var pt = state.PeekTo("]");
            return peekTag == "[credit:" && pt != null && pt.Length > 8 && !pt.Contains("\n");
        }

        public override INode Parse(Parser parser, State state, string scope)
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
                Type = WikiRevisionCredit.TypeCredit
            };

            var sections = str.Split('|');
            foreach (var section in sections)
            {
                var spl = section.Split(new[] {':'}, 2);
                var key = spl[0];
                var val = spl.Length > 1 ? spl[1] : "";
                switch (key) {
                    case "credit":
                        credit.Description = val;
                        break;
                    case "user":
                        if (int.TryParse(val, out var uid)) credit.UserID = uid;
                        break;
                    case "name":
                        credit.Name = val;
                        break;
                    case "url":
                        credit.Url = val;
                        break;
                }
            }

            state.SkipWhitespace();
            return new MetadataNode("WikiCredit", credit);
        }
    }
}