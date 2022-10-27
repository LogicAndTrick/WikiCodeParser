using LogicAndTrick.WikiCodeParser.Elements;
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
        TestNormalised(input, output);
    }

    [TestMethod]
    public void TestStandardWeaponsProgrammingPage()
    {
        var input = Resources.StandardWeaponsProgrammingPageInput;
        var output = Resources.StandardWeaponsProgrammingPageHtml;
        TestNormalised(input, output);
    }

    [TestMethod]
    public void TestLightTutorialPage()
    {
        var input = Resources.LightTutorialInput;
        var output = Resources.LightTutorialHtml;
        TestNormalised(input, output);
    }

    [TestMethod]
    public void TestAdvancedTerrainTutorialPage()
    {
        var input = Resources.AdvancedTerrainTutorialInput;
        var output = Resources.AdvancedTerrainTutorialHtml;
        TestNormalised(input, output);
    }

    private static void TestNormalised(string input, string output)
    {
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
            Assert.AreEqual(ex, ac, $"\n\nMatch failed on line {i + 1}.\n" +
                                    $"Expected: {ex}\n" +
                                    $"Actual  : {ac}");
        }
    }

    private static string Normalise(string text)
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

        text = Regex.Replace(text, "((?:href|src)=\"[^\"]*)%28", "$1(");
        text = Regex.Replace(text, "((?:href|src)=\"[^\"]*)%29", "$1)");
        text = Regex.Replace(text, "((?:href|src)=\"[^\"]*)%3A", "$1:");

        text = text.Replace("http://localhost:82", "https://twhl.info");
        text = text.Replace("localhost:82", "twhl.info");
        text = text.Replace("°", "&#176;");

        text = Regex.Replace(text, "(<pre[^>]*><code>)([^<]*)(</code></pre>)", match => match.Groups[1].Value + String.Join("\n", PreElement.FixCodeIndentation(match.Groups[2].Value.Split('\n').ToList())).Trim() + match.Groups[3].Value);

        return text;
    }
}
