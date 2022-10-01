using System.Text.RegularExpressions;

namespace LogicAndTrick.WikiCodeParser.Tests;

[TestClass]
public class BigEverythingTest
{
    [TestMethod]
    public void TestTwhlFormattingWikiPage()
    {
        var input = Resources.TwhlFormattingWikiPageInput;
        var output = Resources.TwhlFormattingWikiPageHtml;


        var config = ParserConfiguration.Default();
        var parser = new Parser(config);

        var result = parser.ParseResult(input);
        var resultHtml = result.ToHtml();

        // normalise some stuff
        output = Normalise(output);
        resultHtml = Normalise(resultHtml);

        var expectedLines = output.Split('\n');
        var actualLines = resultHtml.Split('\n');

        for (var i = 0; i < expectedLines.Length; i++)
        {
            var ex = expectedLines[i];
            var ac = actualLines[i];
            Assert.AreEqual(ex, ac, $"\n\nMatch failed on line {i+1}.\n" +
                                    $"Expected: {ex}\n" +
                                    $"Actual  : {ac}");
        }
    }

    private string Normalise(string text)
    {
        text = text.Replace("\r", "");
        text = Regex.Replace(text, "&#0+([0-9a-eA-E]+);", "&#$1;");
        text = Regex.Replace(text, "<br ?/?>\n*", "<br/>\n");
        text = Regex.Replace(text, "(</?tr>)\n*", "$1\n");
        text = Regex.Replace(text, "(</?[ou]l>)\n*", "$1\n");
        text = Regex.Replace(text, "</li>\n*", "</li>\n");
        text = Regex.Replace(text, "(</?t[hd]>)\n*", "$1");
        text = Regex.Replace(text, @"(\u00a9|\u00ae|[\u2000-\u3300]|\ud83c[\ud000-\udfff]|\ud83d[\ud000-\udfff]|\ud83e[\ud000-\udfff])", match => System.Web.HttpUtility.HtmlEncode(match.Value));

        text = Regex.Replace(text, "class='([^'<>]*)'", "class=\"$1\"");
        text = Regex.Replace(text, "data-youtube-id='([^'<>]*)'", "data-youtube-id=\"$1\"");
        text = Regex.Replace(text, "style='([^'<>]*)'", match => $"style=\"{match.Groups[1].Value.Replace('"', '\'')}\"");

        text = Regex.Replace(text, "\n*(<blockquote>)", "\n$1");
        text = Regex.Replace(text, "(</blockquote>)\n*", "$1\n");

        text = Regex.Replace(text, "\n*(<div.*?>)\n*", "\n$1\n");
        text = Regex.Replace(text, "\n*</div>\n*", "\n</div>\n");

        text = Regex.Replace(text, "(color: .*?;)(font-size:)", "$1 $2");
        text = Regex.Replace(text, "```\n</code>", "```</code>");

        return text;
    }
}
