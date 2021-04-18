namespace WikiCodeParser.Models
{
    public class WikiRevisionBook
    {
        public int ID { get; set; }
        public int RevisionID { get; set; }
        public string BookName { get; set; }
        public string ChapterName { get; set; }
        public int ChapterNumber { get; set; }
        public int PageNumber { get; set; }
    }
}