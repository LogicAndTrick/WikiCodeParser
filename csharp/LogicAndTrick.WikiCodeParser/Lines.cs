namespace LogicAndTrick.WikiCodeParser
{
    /// <summary>
    /// Line-based state tracker for a multi-line string
    /// </summary>
    public class Lines
    {
        public string[] Content { get; set; }
        public int Index { get; set; }
        private readonly int _startSourceLine;
        public int SourceLine => _startSourceLine == 0 ? 0 : _startSourceLine + System.Math.Max(0, Index);

        public Lines(int startSourceLine, string content)
        {
            Content = content.Split('\n');
            Index = -1;
            _startSourceLine = startSourceLine;
        }

        public void Back() => Index--;
        public bool Next() => ++Index < Content.Length;
        public string Value() => Content[Index];
        public int Current() => Index;
        public void SetCurrent(int index) => Index = index;
        public bool IsEmpty() => string.IsNullOrWhiteSpace(Value());
    }
}