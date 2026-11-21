using System;
using System.Collections.Generic;
using System.Linq;

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

        private readonly IReadOnlyList<int> _newlinePositions;
        private readonly int _startSourceLine;
        private readonly int _startSourceColumn;

        public State(int startSourceLine, int startSourceColumn, string text)
        {
            Text = text;
            Index = 0;

            _startSourceLine = startSourceLine;
            _startSourceColumn = startSourceColumn;
            // get the positions of every newline character, so we can count them
            var nlpos = new List<int> { -1 }; // lines are 1-indexed, we always start with a newline
            var idx = text.IndexOf('\n');
            while (idx >= 0)
            {
                nlpos.Add(idx);
                idx = text.IndexOf('\n', idx + 1);
            }
            _newlinePositions = nlpos;
        }

        public int SourceLine => _startSourceLine + _newlinePositions.Count(x => x < Index) - 1; // the input line is 1-indexed too, so subtract 1 here.
        public int SourceColumn
        {
            get
            {
                var last = _newlinePositions.LastOrDefault(x => x < Index) + 1;
                return Index - last + (last == 0 ? _startSourceColumn : 0);
            }
        }
        public Position SourcePosition => new Position(SourceLine, SourceColumn);

        public string ScanTo(string find, StringComparison comparison = StringComparison.Ordinal)
        {
            var pos = Text.IndexOf(find, Index, comparison);
            if (pos < 0) pos = Length;
            var ret = Text.Substring(Index, pos - Index);
            Index = pos;
            return ret;
        }

        public string ScanToAny(char[] find, StringComparison comparison = StringComparison.Ordinal)
        {
            var pos = -1;
            foreach (var f in find)
            {
                var idx = Text.IndexOf(f.ToString(), Index, comparison);
                if (idx >= 0) pos = pos < 0 ? idx : Math.Min(pos, idx);
            }
            if (pos < 0) pos = Length;
            var ret = Text.Substring(Index, pos - Index);
            Index = pos;
            return ret;
        }

        public string ScanToEnd()
        {
            var ret = Text.Substring(Index, Length - Index);
            Index = Length;
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

        public string PeekToAny(char[] find)
        {
            var pos = Text.IndexOfAny(find, Index);
            if (pos < 0) return null;
            return Text.Substring(Index, pos - Index);
        }

        public void Seek(int index, bool fromStart)
        {
            Index = fromStart ? index : Index + index;
        }

        public string Peek(int count)
        {
            if (count < 0)
            {
                var start = Math.Max(0, Index + count);
                return Text.Substring(start, Index - start);
            }
            else
            {
                if (Index + count > Length) count = Length - Index;
                return Text.Substring(Index, count);
            }
        }

        public char Next()
        {
            if (Index >= Length) return '\0';
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