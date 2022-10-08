using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LogicAndTrick.WikiCodeParser.Elements;
using LogicAndTrick.WikiCodeParser.Nodes;
using LogicAndTrick.WikiCodeParser.Tags;

namespace LogicAndTrick.WikiCodeParser.Tests
{
    [TestClass]
    public class ParserTest
    {
        [TestMethod]
        public void TestParseResultEmpty()
        {
            var parser = new Parser(new ParserConfiguration());
            var result = parser.ParseResult("");
            var coll = (NodeCollection)result.Content;
            Assert.AreEqual(0, coll.Nodes.Count);
        }

        [TestMethod]
        public void TestParseResultText()
        {
            var parser = new Parser(new ParserConfiguration());
            var result = parser.ParseResult("A");
            var coll = (NodeCollection)result.Content;
            Assert.AreEqual(1, coll.Nodes.Count);
            var ptn = (PlainTextNode)coll.Nodes[0];
            Assert.AreEqual("A", ptn.Text);
        }

        [TestMethod]
        public void TestParseElementsEmpty()
        {
            var parser = new Parser(new ParserConfiguration());
            var data = new ParseData();
            var coll = (NodeCollection) parser.ParseElements(data, "A", "");
            Assert.AreEqual(1, coll.Nodes.Count);
            var ptn = (PlainTextNode)coll.Nodes[0];
            Assert.AreEqual("A", ptn.Text);
        }

        [TestMethod]
        public void TestParseElementsOne()
        {
            var config = new ParserConfiguration();
            config.Elements.Add(new PreElement());
            var parser = new Parser(config);
            var data = new ParseData();
            var coll = (NodeCollection) parser.ParseElements(data, "A\n[pre]\nB\n[/pre]\nC", "");
            Assert.AreEqual(5, coll.Nodes.Count);
            Assert.AreEqual("A", ((PlainTextNode)coll.Nodes[0]).Text);
            Assert.AreEqual("\n", ((UnprocessablePlainTextNode)coll.Nodes[1]).Text);
            Assert.AreEqual("\n", ((UnprocessablePlainTextNode)coll.Nodes[3]).Text);
            Assert.AreEqual("C", ((PlainTextNode)coll.Nodes[4]).Text);

            var html = (HtmlNode)coll.Nodes[2];
            Assert.AreEqual("<pre><code>", html.HtmlBefore);
            Assert.AreEqual("</code></pre>", html.HtmlAfter);
            Assert.AreEqual("B", ((UnprocessablePlainTextNode)html.Content).Text);
        }

        [TestMethod]
        public void TestParseTagsEmpty()
        {
            var parser = new Parser(new ParserConfiguration());
            var data = new ParseData();
            var coll = (NodeCollection) parser.ParseTags(data, "A", "", TagParseContext.Block);
            Assert.AreEqual(1, coll.Nodes.Count);
            var ptn = (PlainTextNode)coll.Nodes[0];
            Assert.AreEqual("A", ptn.Text);
        }

        [TestMethod]
        public void TestParseTagsOne()
        {
            var config = new ParserConfiguration();
            config.Tags.Add(new Tag("test", "span", "test"));
            var parser = new Parser(config);
            var data = new ParseData();
            var coll = (NodeCollection) parser.ParseTags(data, "A [test]B[/test] C", "", TagParseContext.Block);
            Assert.AreEqual(3, coll.Nodes.Count);
            Assert.AreEqual("A ", ((PlainTextNode)coll.Nodes[0]).Text);
            Assert.AreEqual(" C", ((PlainTextNode)coll.Nodes[2]).Text);

            var html = (HtmlNode)coll.Nodes[1];
            Assert.AreEqual("<span class=\"test\">", html.HtmlBefore);
            Assert.AreEqual("</span>", html.HtmlAfter);
            Assert.AreEqual("<span class=\"test\">B</span>", html.ToHtml());
            Assert.AreEqual("B", html.ToPlainText());
        }

        [TestMethod]
        public void TestTrimWhitespace()
        {
            var coll = new NodeCollection();
            coll.Nodes.Add(PlainTextNode.Empty);
            coll.Nodes.Add(UnprocessablePlainTextNode.NewLine);
            coll.Nodes.Add(new PlainTextNode("A"));
            coll.Nodes.Add(PlainTextNode.Empty);
            coll.Nodes.Add(UnprocessablePlainTextNode.NewLine);
            coll.Nodes.Add(new PlainTextNode("B"));
            coll.Nodes.Add(PlainTextNode.Empty);
            coll.Nodes.Add(UnprocessablePlainTextNode.NewLine);

            Parser.TrimWhitespace(coll, true, true);

            Assert.AreEqual("RemovedNode", coll.Nodes[0].GetType().Name);
            Assert.AreEqual("RemovedNode", coll.Nodes[1].GetType().Name);
            Assert.AreEqual("PlainTextNode", coll.Nodes[2].GetType().Name);
            Assert.AreEqual("PlainTextNode", coll.Nodes[3].GetType().Name);
            Assert.AreEqual("UnprocessablePlainTextNode", coll.Nodes[4].GetType().Name);
            Assert.AreEqual("PlainTextNode", coll.Nodes[5].GetType().Name);
            Assert.AreEqual("RemovedNode", coll.Nodes[6].GetType().Name);
            Assert.AreEqual("RemovedNode", coll.Nodes[7].GetType().Name);
        }

        [TestMethod]
        public void TestFlattenNestedNodeCollections()
        {
            var l3 = PlainTextNode.Empty;
            var l2 = new NodeCollection(l3);
            var l1 = new NodeCollection(l2);
            var coll = new NodeCollection(l1);
            var html = new HtmlNode("", coll, "");

            Parser.FlattenNestedNodeCollections(html);
            Assert.AreEqual(l3, html.Content);
        }
    }
}
