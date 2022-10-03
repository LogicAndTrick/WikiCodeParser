using LogicAndTrick.WikiCodeParser.Elements;
using LogicAndTrick.WikiCodeParser.Nodes;
using LogicAndTrick.WikiCodeParser.Processors;
using LogicAndTrick.WikiCodeParser.Tags;

namespace LogicAndTrick.WikiCodeParser.Tests;

[TestClass]
public class BasicTests
{
    [TestMethod]
    public void TestHtmlEscapingOutsideTag()
    {
        var parser = new Parser(new ParserConfiguration());
        var result = parser.ParseResult("1 & 2");
        Assert.IsInstanceOfType(result.Content, typeof(NodeCollection));
        var leaves = GetLeaves(result.Content);
        Assert.AreEqual(1, leaves.Count);
        var node = leaves[0];
        Assert.IsInstanceOfType(node, typeof(PlainTextNode));
        var ptnode = (PlainTextNode)node;
        Assert.AreEqual("1 & 2", ptnode.Text);
        Assert.AreEqual("1 &amp; 2", ptnode.ToHtml());
        Assert.AreEqual("1 & 2", ptnode.ToPlainText());
    }

    [TestMethod]
    public void TestHtmlEscapingInsideTag()
    {
        var config = new ParserConfiguration();
        config.Tags.Add(new QuickLinkTag());
        var parser = new Parser(config);
        var result = parser.ParseResult("[https://example.com|ex&ple]");
        Assert.IsInstanceOfType(result.Content, typeof(NodeCollection));
        var leaves = CollapseCollections(result.Content);
        Assert.AreEqual(1, leaves.Count);
        var node = leaves[0];
        Assert.IsInstanceOfType(node, typeof(HtmlNode));
        var htnode = (HtmlNode)node;
        Assert.AreEqual("<a href=\"https://example.com\">", htnode.HtmlBefore);
        Assert.AreEqual("</a>", htnode.HtmlAfter);
        var content = CollapseCollections(htnode.Content);
        Assert.AreEqual(1, content.Count);
        Assert.IsInstanceOfType(content[0], typeof(UnprocessablePlainTextNode));
        var ptnode = (UnprocessablePlainTextNode)content[0];
        Assert.AreEqual("ex&ple", ptnode.Text);
    }

    [TestMethod]
    public void TestBlockNewLines()
    {
        var input = "a\n\n\n\n= b\n\n\n\nc\nd\n\ne";
        var output = "a\n<h1 id=\"b\">b</h1>\nc<br/>\nd<br/>\n<br/>\ne";

        var config = new ParserConfiguration();
        config.Elements.Add(new MdHeadingElement());
        config.Processors.Add(new NewLineProcessor());
        var parser = new Parser(config);
        var result = parser.ParseResult(input);

        Assert.AreEqual(output, result.ToHtml());
    }

    [TestMethod]
    public void TestNewLineAfterTag()
    {
        var input = "A [code]B[/code]\n\nC";
        var output = "A <code>B</code><br/>\n<br/>\nC";

        var config = new ParserConfiguration();
        config.Tags.Add(new CodeTag());
        config.Processors.Add(new NewLineProcessor());
        var parser = new Parser(config);
        var result = parser.ParseResult(input);

        Assert.AreEqual(output, result.ToHtml());
    }

    [TestMethod]
    public void TestSmiliesList()
    {
        var input = ":aggrieved:\n:glad:\n";
        var output = "<img class=\"smiley\" src=\"https://twhl.info/images/smilies/aggrieved.png\" alt=\":aggrieved:\" /><br/>\n<img class=\"smiley\" src=\"https://twhl.info/images/smilies/glad.png\" alt=\":glad:\" />";

        var config = new ParserConfiguration();
        config.Processors.Add(new SmiliesProcessor().AddDefault());
        config.Processors.Add(new NewLineProcessor());
        var parser = new Parser(config);
        var result = parser.ParseResult(input);

        Assert.AreEqual(output, result.ToHtml());
    }

    [TestMethod]
    public void TestMultipleNewLines()
    {
        var input = "a\n\n\nb\n\nc\nd\n\n";
        var output = "a<br/>\n<br/>\nb<br/>\n<br/>\nc<br/>\nd";

        var config = new ParserConfiguration();
        config.Processors.Add(new NewLineProcessor());
        var parser = new Parser(config);
        var result = parser.ParseResult(input);

        Assert.AreEqual(output, result.ToHtml());
    }

    [TestMethod]
    public void TestMultipleHeadings()
    {
        var input = "= One\n==Two";
        var output = "<h1 id=\"One\">One</h1>\n<h2 id=\"Two\">Two</h2>";

        var config = new ParserConfiguration();
        config.Elements.Add(new MdHeadingElement());
        config.Processors.Add(new NewLineProcessor());
        var parser = new Parser(config);
        var result = parser.ParseResult(input);

        Assert.AreEqual(output, result.ToHtml());
    }

    [TestMethod]
    public void TestQuoteNewLines()
    {
        var input = "before\n\n[quote]quote[/quote]\n\nafter";
        var output = "before\n<blockquote>quote</blockquote>\nafter";

        var config = new ParserConfiguration();
        config.Tags.Add(new QuoteTag());
        config.Processors.Add(new TrimWhitespaceAroundBlockNodesProcessor());
        config.Processors.Add(new NewLineProcessor());
        var parser = new Parser(config);
        var result = parser.ParseResult(input);

        Assert.AreEqual(output, result.ToHtml());
    }

    [TestMethod]
    public void TestListsAndTables()
    {
        var input = "- one ^\n" +
                    "  two";
        var output = "<ul>\n<li>one<br/>\ntwo</li>\n</ul>\n";

        var config = new ParserConfiguration();
        config.Elements.Add(new MdTableElement());
        config.Elements.Add(new MdListElement());
        config.Processors.Add(new TrimWhitespaceAroundBlockNodesProcessor());
        config.Processors.Add(new NewLineProcessor());
        var parser = new Parser(config);
        var result = parser.ParseResult(input);

        Assert.AreEqual(output, result.ToHtml());
    }

    [TestMethod]
    public void TestNewLinesBeforeImage()
    {
        var input = "Before\n\n[img]https://example.com/example.png[/img]\n\nAfter";
        var output = "Before\n<div class=\"embedded image\"><span class=\"caption-panel\"><img class=\"caption-body\" src=\"https://example.com/example.png\" alt=\"User posted image\" /></span></div>\nAfter";

        var config = ParserConfiguration.Default();
        var parser = new Parser(config);
        var result = parser.ParseResult(input);

        Assert.AreEqual(output, result.ToHtml());
    }

    [TestMethod]
    public void TestBoldAndNewLinesBeforeImage()
    {
        var input = "*Bold*\n\n[img]https://example.com/example.png[/img]\n\nAfter";
        var output = "<strong>Bold</strong>\n<div class=\"embedded image\"><span class=\"caption-panel\"><img class=\"caption-body\" src=\"https://example.com/example.png\" alt=\"User posted image\" /></span></div>\nAfter";

        var config = ParserConfiguration.Default();
        var parser = new Parser(config);
        var result = parser.ParseResult(input);

        Assert.AreEqual(output, result.ToHtml());
    }

    [TestMethod]
    public void TestCategoriesAndWhitespace()
    {
        var input = "[cat:A]\n[cat:B]\n\n= Heading";
        var output = "<h1 id=\"Heading\">Heading</h1>";

        var config = ParserConfiguration.Default();
        var parser = new Parser(config);
        var result = parser.ParseResult(input);

        Assert.AreEqual(output, result.ToHtml());
    }

    private static IList<INode> GetLeaves(INode root)
    {
        var list = new List<INode>();
        GetLeavesRecursive(list, root);
        return list;
    }

    private static void GetLeavesRecursive(ICollection<INode> list, INode node)
    {
        var children = node.GetChildren();
        if (children.Count == 0)
        {
            list.Add(node);
        }
        else
        {
            foreach (var child in children) GetLeavesRecursive(list, child);
        }
    }

    private static IList<INode> CollapseCollections(INode root)
    {
        var list = new List<INode>();
        CollapseCollectionsRecursive(list, root);
        return list;
    }

    private static void CollapseCollectionsRecursive(ICollection<INode> list, INode node)
    {
        if (node is NodeCollection coll) coll.Nodes.ForEach(x => CollapseCollectionsRecursive(list, x));
        else list.Add(node);
    }
}