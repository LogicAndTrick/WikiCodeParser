using LogicAndTrick.WikiCodeParser.Elements;
using LogicAndTrick.WikiCodeParser.Processors;

namespace LogicAndTrick.WikiCodeParser.Tests.Elements;

[TestClass]
public class RefElementTest
{
    private static Parser CreateParser()
    {
        var config = new ParserConfiguration();
        config.Elements.Add(new MdTableElement());
        config.Elements.Add(new RefElement());
        config.Processors.Add(new NewLineProcessor());
        return new Parser(config);
    }

    [TestMethod]
    public void UnusedRefTest()
    {
        var input = "[ref=test]aaaa[/ref]";
        var output = "";
        var parser = CreateParser();
        var result = parser.ParseResult(input);
        Assert.AreEqual(output, result.ToHtml());
    }

    [TestMethod]
    public void UsedRefTest1()
    {
        var input = "\n|-:ref=test\n[ref=test]aaaa[/ref]";
        var output = "<table class=\"table table-bordered\"><tr>\n<td>aaaa</td>\n</tr>\n</table>";
        var parser = CreateParser();
        var result = parser.ParseResult(input);
        Assert.AreEqual(output, result.ToHtml());
    }

    [TestMethod]
    public void UsedRefTest2()
    {
        var input = "\n|-:ref=test\n[ref=test]\naaaa[/ref]";
        var output = "<table class=\"table table-bordered\"><tr>\n<td>aaaa</td>\n</tr>\n</table>";
        var parser = CreateParser();
        var result = parser.ParseResult(input);
        Assert.AreEqual(output, result.ToHtml());
    }

    [TestMethod]
    public void UsedRefTest3()
    {
        var input = "\n|-:ref=test\n[ref=test]aaaa\n[/ref]";
        var output = "<table class=\"table table-bordered\"><tr>\n<td>aaaa</td>\n</tr>\n</table>";
        var parser = CreateParser();
        var result = parser.ParseResult(input);
        Assert.AreEqual(output, result.ToHtml());
    }

    [TestMethod]
    public void UsedRefTest4()
    {
        var input = "\n|-:ref=test\n[ref=test]\naaaa\n[/ref]";
        var output = "<table class=\"table table-bordered\"><tr>\n<td>aaaa</td>\n</tr>\n</table>";
        var parser = CreateParser();
        var result = parser.ParseResult(input);
        Assert.AreEqual(output, result.ToHtml());
    }
}