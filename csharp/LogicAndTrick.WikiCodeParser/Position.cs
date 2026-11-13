namespace LogicAndTrick.WikiCodeParser
{
    public struct Position
    {
        public static readonly Position Zero = new Position(0, 0);

        public int Line { get; set; }
        public int Column { get; set; }

        public Position(int line, int column)
        {
            Line = line;
            Column = column;
        }
    }
}