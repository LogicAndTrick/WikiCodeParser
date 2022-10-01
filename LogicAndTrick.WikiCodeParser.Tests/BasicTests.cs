using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LogicAndTrick.WikiCodeParser.Nodes;
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
        Assert.IsInstanceOfType(content[0], typeof(PlainTextNode));
        var ptnode = (PlainTextNode)content[0];
        Assert.AreEqual("ex&ple", ptnode.Text);
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