namespace LogicAndTrick.WikiCodeParser.Tests.Tags;

public class TagTestData
{
    public string Input { get; set; }
    public string ExpectedHtml { get; set; }
    public string ExpectedPlaintext { get; set; }
    public List<KeyValuePair<string, object>> ExpectedMetaData { get; set; }
    public bool IsBlock { get; set; }

    internal string BlockSpace => IsBlock ? "" : " ";
    internal string BlockNewLine => IsBlock ? "" : "\n";

    public TagTestData(string input, string expectedHtml, string expectedPlaintext)
    {
        Input = input;
        ExpectedHtml = expectedHtml;
        ExpectedPlaintext = expectedPlaintext;
        ExpectedMetaData = new List<KeyValuePair<string, object>>();
    }
}