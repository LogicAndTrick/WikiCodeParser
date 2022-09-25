using LogicAndTrick.WikiCodeParser.Nodes;

namespace LogicAndTrick.WikiCodeParser.Tags
{
    public class WikiCategoryTag : Tag
    {
        public WikiCategoryTag()
        {
            Token = null;
            Element = "";
        }

        public override bool Matches(State state, string token)
        {
            var peekTag = state.Peek(5);
            var pt = state.PeekTo("]");
            return peekTag == "[cat:" && pt != null && pt.Length > 5 && !pt.Contains("\n");
        }

        public override INode Parse(Parser parser, ParseData data, State state, string scope)
        {
            var index = state.Index;
            if (state.ScanTo(":") != "[cat" || state.Next() != ':')
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

            state.SkipWhitespace();
            return new MetadataNode("WikiCategory", str.Trim());
        }
    }
}