namespace LogicAndTrick.WikiCodeParser.Models
{
    public class WikiRevisionCredit
    {
        public const char TypeCredit = 'c';
        public const char TypeArchive = 'a';
        public const char TypeFull = 'f';

        public int ID { get; set; }
        public char Type { get; set; }
        public int RevisionID { get; set; }
        public string Description { get; set; }
        public int? UserID { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public string WaybackUrl { get; set; }
    }
}
