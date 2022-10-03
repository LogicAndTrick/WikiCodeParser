using System;

namespace LogicAndTrick.WikiCodeParser
{
    /// <summary>
    /// Character-indexed state tracker for a string
    /// </summary>
    public class State
    {
        public string Text { get; }
        public int Length => Text.Length;
        public int Index { get; private set; }
        public bool Done => Index >= Length;

        public State(string text)
        {
            Text = text;
            Index = 0;
        }

        public string ScanTo(string find, StringComparison comparison = StringComparison.Ordinal)
        {
            var pos = Text.IndexOf(find, Index, comparison);
            if (pos < 0) pos = Length;
            var ret = Text.Substring(Index, pos - Index);
            Index = pos;
            return ret;
        }

        public void SkipWhitespace()
        {
            while (Index < Length && Char.IsWhiteSpace(Text[Index])) Index++;
        }

        public string PeekTo(string find)
        {
            var pos = Text.IndexOf(find, Index, StringComparison.Ordinal);
            if (pos < 0) return null;
            return Text.Substring(Index, pos - Index);
        }

        public void Seek(int index, bool fromStart)
        {
            Index = fromStart ? index : Index + index;
        }

        public string Peek(int count)
        {
            if (Index + count > Length) count = Length - Index;
            return Text.Substring(Index, count);
        }

        public char Next()
        {
            return Text[Index++];
        }

        public string GetToken()
        {
            if (Done || Text[Index] != '[') return null;
            var found = false;
            var tok = "";
            for (var i = Index + 1; i < Math.Min(Index + 10, Length); i++)
            {
                var c = Text[i];
                if (c == ' ' || c == '=' || c == ']')
                {
                    found = tok.Length > 0;
                    break;
                }

                tok += c;
            }

            return found ? tok.ToLower() : null;
        }
    }
}