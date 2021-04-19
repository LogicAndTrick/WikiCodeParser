using WikiCodeParser.Nodes;
using WikiCodeParser.Models;

namespace WikiCodeParser.Tags
{
    public class WikiBookTag : Tag
    {
        public WikiBookTag()
        {
            Token = null;
            Element = "";
        }

        public override bool Matches(State state, string token)
        {
            var peekTag = state.Peek(6);
            var pt = state.PeekTo("]");
            return peekTag == "[book:" && pt != null && pt.Length > 6 && !pt.Contains("\n");
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

            var book = new WikiRevisionBook();

            var sections = str.Split('|');
            foreach (var section in sections)
            {
                var spl = section.Split(new[] {':'}, 2);
                var key = spl[0];
                var val = spl.Length > 1 ? spl[1] : "";
                switch (key) {
                    case "book":
                        book.BookName = val;
                        break;
                    case "chapter":
                        book.ChapterName = val;
                        break;
                    case "chapternumber":
                        if (int.TryParse(val, out var cn)) book.ChapterNumber = cn;
                        break;
                    case "pagenumber":
                        if (int.TryParse(val, out var pn)) book.PageNumber = pn;
                        break;
                }
            }

            state.SkipWhitespace();
            return new MetadataNode("WikiBook", book);
        }
    }
}