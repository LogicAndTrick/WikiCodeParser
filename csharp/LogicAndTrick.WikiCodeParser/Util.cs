namespace LogicAndTrick.WikiCodeParser
{
    public static class Util
    {
        static readonly char[] TrimChars = new char[] { ' ', '\t', '\n', '\r', '\0', '\x0B' };

        public static string TrimStart(string str)
        {
            return str.TrimStart(TrimChars);
        }

        public static string TrimEnd(string str)
        {
            return str.TrimEnd(TrimChars);
        }

        public static string Trim(string str)
        {
            return str.Trim(TrimChars);
        }
    }
}