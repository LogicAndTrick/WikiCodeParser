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
        config.Processors.Add(new SmiliesProcessor("https://twhl.info/images/smilies/{0}.png").AddTwhl());
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
    public void TestRefElement()
    {
        DefaultConfigurationTest(@"
|- :ref=cell1
[ref=cell1]1
2[/ref]
".Trim(), @"
<table class=""table table-bordered""><tr>
<td>1<br/>
2</td>
</tr>
</table>
".Trim());
    }

    [TestMethod]
    public void TestNewLinesBeforeImage()
    {
        var input = "Before\n\n[img]https://example.com/example.png[/img]\n\nAfter";
        var output = "Before\n<div class=\"embedded image\"><span class=\"caption-panel\"><img class=\"caption-body\" src=\"https://example.com/example.png\" alt=\"User posted image\" /></span></div>\nAfter";

        var config = ParserConfiguration.Twhl();
        var parser = new Parser(config);
        var result = parser.ParseResult(input);

        Assert.AreEqual(output, result.ToHtml());
    }

    [TestMethod]
    public void TestBoldAndNewLinesBeforeImage()
    {
        var input = "*Bold*\n\n[img]https://example.com/example.png[/img]\n\nAfter";
        var output = "<strong>Bold</strong>\n<div class=\"embedded image\"><span class=\"caption-panel\"><img class=\"caption-body\" src=\"https://example.com/example.png\" alt=\"User posted image\" /></span></div>\nAfter";

        var config = ParserConfiguration.Twhl();
        var parser = new Parser(config);
        var result = parser.ParseResult(input);

        Assert.AreEqual(output, result.ToHtml());
    }

    [TestMethod]
    public void TestCategoriesAndWhitespace()
    {
        var input = "[cat:A]\n[cat:B]\n\n= Heading";
        var output = "<h1 id=\"Heading\">Heading</h1>";

        var config = ParserConfiguration.Twhl();
        var parser = new Parser(config);
        var result = parser.ParseResult(input);

        Assert.AreEqual(output, result.ToHtml());
    }

    [TestMethod]
    public void TestNestedBlockTagsInsideInline()
    {
        // these are block tags and shouldn't be allowed in inline tags
        DefaultConfigurationTest("[font=blue][img]https://example.com/example.png[/img][/font]", @"<span style=""color: blue;"">[img]https://example.com/example.png[/img]</span>");
        DefaultConfigurationTest("[font=blue][youtube]123[/youtube][/font]", @"<span style=""color: blue;"">[youtube]123[/youtube]</span>");
        DefaultConfigurationTest("[font=blue][pre]123[/pre][/font]", @"<span style=""color: blue;"">[pre]123[/pre]</span>");
    }

    [TestMethod]
    public void TestNestedTagsInsideCodeTag()
    {
        // all syntax should be ignored inside a code tag
        DefaultConfigurationTest("[code][font=red]test[/font][/code]", @"<code>[font=red]test[/font]</code>");
    }

    [TestMethod]
    public void TestNestedInlineTagsInsideInline()
    {
        // these are inline, should be allowed
        DefaultConfigurationTest("[font=blue][simg]https://example.com/example.png[/simg][/font]", @"<span style=""color: blue;""><span class=""embedded image inline""><span class=""caption-panel""><img class=""caption-body"" src=""https://example.com/example.png"" alt=""User posted image"" /></span></span></span>");
        DefaultConfigurationTest("[font=blue][url]https://example.com[/url][/font]", @"<span style=""color: blue;""><a href=""https://example.com"">https://example.com</a></span>");
        DefaultConfigurationTest("[font=blue][https://example.com][/font]", @"<span style=""color: blue;""><a href=""https://example.com"">https://example.com</a></span>");
    }

    [TestMethod]
    public void TestAllowNestedFontTags()
    {
        var input = "A[font=red]B[font=blue]C[/font]D[/font]E";
        var expected = "A<span style=\"color: red;\">B<span style=\"color: blue;\">C</span>D</span>E";

        var config = ParserConfiguration.Twhl();
        config.Tags.Find(x => x is FontTag)!.IsNested = true;

        var parser = new Parser(config);
        var result = parser.ParseResult(input);
        Assert.AreEqual(expected, result.ToHtml());
    }

    [TestMethod]
    public void TestPreventErrors()
    {
        var input = "[font=\n[pre]test[/pre]";
        var expected = "[font=\n<pre><code>test</code></pre>";
        DefaultConfigurationTest(input, expected);
    }

    [TestMethod]
    public void TestCaseInsensitive()
    {
        DefaultConfigurationTest("[FONT=RED]a[/font]", "<span style=\"color: RED;\">a</span>");
        DefaultConfigurationTest("[FONT=#AAbb11]a[/font]", "<span style=\"color: #AAbb11;\">a</span>");
        DefaultConfigurationTest_MustChange("[FONT=RED]a[/font]");
        DefaultConfigurationTest_MustChange("[B]a[/B]");
        DefaultConfigurationTest_MustChange("[IMG]https://example.com/example.png[/IMG]");
        DefaultConfigurationTest_MustChange("[URL]https://example.com[/URL]");
        DefaultConfigurationTest_MustChange("[URL=https://example.com]a[/URL]");
        DefaultConfigurationTest_MustChange("[QUOTE]a[/QUOTE]");
        DefaultConfigurationTest_MustChange("[SPOILER]a[/SPOILER]");
        DefaultConfigurationTest_MustChange("[YOUTUBE]aaaaaaaaa[/YOUTUBE]");
    }

    [TestMethod]
    public void TestNestedMarkdown()
    {
        DefaultConfigurationTest("a /Test/ b", "a <em>Test</em> b");

        // Bold/italic/underline/strikethrough can be nested
        DefaultConfigurationTest("a /*Test*/ b ", "a <em><strong>Test</strong></em> b");
        DefaultConfigurationTest("a */Test/* b ", "a <strong><em>Test</em></strong> b");
        DefaultConfigurationTest("a */Test*/ b ", "a <strong>/Test</strong>/ b");
        DefaultConfigurationTest("a /*Test/* b ", "a <em>*Test</em>* b");

        // Code cannot have nested markdown
        DefaultConfigurationTest("`*Test*`", "<code>*Test*</code>");
        DefaultConfigurationTest("*`Test`*", "<strong><code>Test</code></strong>");
        DefaultConfigurationTest("`*Test`*", "<code>*Test</code>*");
        DefaultConfigurationTest("*`Test*`", "<strong>`Test</strong>`");
    }

    private static void DefaultConfigurationTest_MustChange(string input)
    {
        input = input.Replace("\r", "");

        var config = ParserConfiguration.Twhl();
        var parser = new Parser(config);
        var result = parser.ParseResult(input);
        Assert.AreNotEqual(input, result.ToHtml());
    }

    private static void DefaultConfigurationTest(string input, string expected)
    {
        input = input.Replace("\r", "");
        expected = expected.Replace("\r", "");

        var config = ParserConfiguration.Twhl();
        var parser = new Parser(config);
        var result = parser.ParseResult(input);
        Assert.AreEqual(expected, result.ToHtml());
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