using WikiCodeParser.Nodes;

namespace WikiCodeParser
{
    public class ParseResult : IBBCodeContent
    {
        public BBCodeContent Content { get; set; }

        public ParseResult()
        {
            Content = new BBCodeContent();
        }

        public string ToHtml() => Content.ToHtml();
        public string ToPlainText() => Content.ToPlainText();
    }
}