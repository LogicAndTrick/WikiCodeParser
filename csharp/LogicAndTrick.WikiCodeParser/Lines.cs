namespace LogicAndTrick.WikiCodeParser
{
    /// <summary>
    /// Line-based state tracker for a multi-line string
    /// </summary>
    public class Lines
    {
        public string[] Content { get; set; }
        public int Index { get; set; }

        public Lines(string content)
        {
            Content = content.Split('\n');
            Index = -1;
        }

        public void Back() => Index--;
        public bool Next() => ++Index < Content.Length;
        public string Value() => Content[Index];
        public int Current() => Index;
        public void SetCurrent(int index) => Index = index;
    }
}