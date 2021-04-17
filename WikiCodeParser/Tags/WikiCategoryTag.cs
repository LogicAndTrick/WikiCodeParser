using System.Linq;
using WikiCodeParser.Nodes;

namespace WikiCodeParser.Tags
{
    public class WikiCategoryTag : BBCodeTag
    {
        public override string Token => null;
        public override string Element => "";

        public override bool Matches(State state, string token)
        {
            var peekTag = state.Peek(5);
            var pt = state.PeekTo("]");
            return peekTag == "[cat:" && pt != null && pt.Length > 5 && !pt.Contains('\n');
        }

        public override BBCodeContent Parse(Parser parser, State state, string scope)
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
            var bbcc = new BBCodeContent();
            bbcc.AddMeta("WikiCategory", str);
            return bbcc;
        }
    }
}