using LogicAndTrick.WikiCodeParser.Nodes;
using LogicAndTrick.WikiCodeParser.Tags;

namespace LogicAndTrick.WikiCodeParser.Tests
{
    [TestClass]
    public class LineColumnTests
    {
        public record class Position(string Name, int Line, int Column, params Position[] Children);

        public static void AssertPositionHierarchy(INode node, Position expected)
        {
            if (expected.Line > 0)
            {
                Assert.AreEqual(expected.Line, node.Line, $"Wrong line for '{expected.Name}'");
                Assert.AreEqual(expected.Column, node.Column, $"Wrong column for '{expected.Name}'");
            }

            var children = node.GetChildren();
            Assert.AreEqual(expected.Children.Length, children.Count);

            for (int i = 0; i < expected.Children.Length; i++)
            {
                AssertPositionHierarchy(children[i], expected.Children[i]);
            }
        }

        [TestMethod]
        public void BasicColumns()
        {
            var config = new ParserConfiguration("http://example.com");
            config.Tags.Add(new Tag("b", "strong"));

            var result = new Parser(config).ParseResult("a [b]c[/b] d");

            AssertPositionHierarchy(result.Content,
                new Position("root", 1, 0,
                    new Position("a", 1, 0),
                    new Position("b", 1, 2,
                        new Position("c", 1, 5)
                    ),
                    new Position("d", 1, 10)
                )
            );
        }

        [TestMethod]
        public void BasicLines()
        {
            var config = new ParserConfiguration("http://example.com");
            config.Tags.Add(new Tag("b", "strong"));

            var result = new Parser(config).ParseResult("first\n[b]second[/b]");
            
            AssertPositionHierarchy(result.Content,
                new Position("root", 1, 0,
                    new Position("first", 1, 0),
                    new Position("b", 2, 0,
                        new Position("second", 2, 3)
                    )
                )
            );
        }

        [TestMethod]
        public void BasicWhitespace()
        {
            var config = new ParserConfiguration("http://example.com");
            config.Tags.Add(new Tag("b", "strong"));

            var result = new Parser(config).ParseResult("\n\n\n[b]TEST[/b]");
            
            AssertPositionHierarchy(result.Content,
                new Position("root", 1, 0,
                    new Position("b", 4, 0,
                        new Position("TEST", 4, 3)
                    )
                )
            );
        }

        [TestMethod]
        public void BasicWhitespace2()
        {
            var config = new ParserConfiguration("http://example.com");
            config.Tags.Add(new Tag("b", "strong"));

            var result = new Parser(config).ParseResult("[b]TEST1[/b]\n\n\n\n[b]TEST2[/b]");
            
            AssertPositionHierarchy(result.Content,
                new Position("root", 1, 0,
                    new Position("b1", 1, 0,
                        new Position("TEST1", 1, 3)
                    ),
                    new Position("b2", 5, 0,
                        new Position("TEST2", 5, 3)
                    )
                )
            );
        }
    }
}
