using LogicAndTrick.WikiCodeParser.Elements;
using LogicAndTrick.WikiCodeParser.Processors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicAndTrick.WikiCodeParser.Tests.Elements;

[TestClass]
public class QuoteElementTest
{
    private static Parser CreateParser()
    {
        var config = ParserConfiguration.Twhl();
        return new Parser(config);
    }

    public static IEnumerable<object?[]> GetBalanceQuotesData()
    {
        yield return new object?[] { "[quote]Test[/quote]", "Test" };
        yield return new object?[] { "[quote]Test[/quote][quote]Test[/quote]", "Test" };
        yield return new object?[] { "[quote]Test\n[quote]Test[/quote][/quote]", "Test\n[quote]Test[/quote]" };
        yield return new object?[] { "[quote]Test\n[quote]Test[/quote]Test[/quote]", "Test\n[quote]Test[/quote]Test" };
        yield return new object?[] { "[quote]Test\n[quote]Test[/quote]Test\n[quote]Test[/quote]", null };
        yield return new object?[] { "[quote][quote]Test[/quote]Test[/quote]", "[quote]Test[/quote]Test" };
        yield return new object?[] { "[quote][quote]Test[/quote]\nTest[/quote]", "[quote]Test[/quote]\nTest" };
    }

    [TestMethod]
    [DynamicData(nameof(GetBalanceQuotesData), DynamicDataSourceType.Method)]
    public void BalanceQuotesTest(string input, string output)
    {
        var lines = new Lines(input);
        lines.Next();
        Assert.AreEqual(output, QuoteElement.BalanceQuotes(lines, out _, out _));
    }

    [TestMethod]
    public void BalanceQuotesWithNameTest()
    {
        var input = "[quote=Name]Test[/quote]";
        var output = "Test";
        var author = "Name";
        var lines = new Lines(input);
        lines.Next();
        var result = QuoteElement.BalanceQuotes(lines, out var name, out _);
        Assert.AreEqual(output, result);
        Assert.AreEqual(author, name);
    }

    [TestMethod]
    public void BalanceQuotesWithPostfixTest()
    {
        var input = "[quote]Test[/quote]ASDF";
        var output = "Test";
        var postfix = "ASDF";
        var lines = new Lines(input);
        lines.Next();
        var result = QuoteElement.BalanceQuotes(lines, out _, out var rest);
        Assert.AreEqual(output, result);
        Assert.AreEqual(postfix, rest);
    }

    [TestMethod]
    public void QuoteSimple()
    {
        var input = "[quote]Test[/quote]";
        var output = "\n<blockquote>Test</blockquote>\n";
        var parser = CreateParser();
        var result = parser.ParseResult(input);
        Assert.AreEqual(output, result.ToHtml());
    }
}