
using System.Reflection;
using System.Text;
using System.Text.Json;
using LogicAndTrick.WikiCodeParser.Elements;
using LogicAndTrick.WikiCodeParser.Nodes;
using LogicAndTrick.WikiCodeParser.Processors;
using LogicAndTrick.WikiCodeParser.Tags;
using LogicAndTrick.WikiCodeParser.Tests.TestModels;

namespace LogicAndTrick.WikiCodeParser.Tests;

[TestClass]
[TestCategory("Implementation")]
public class ImplementationTests
{
    private readonly static List<int> _lineOffsets = [0, 1, 5, 10];

    public static IEnumerable<(string name, int lineOffset)> GetTestCases()
    {
        return TestCaseUtils.GetTestCaseNames("implementations").SelectMany(x => _lineOffsets.Select(y => (x, y)));
    }

    public static string? GetTestCaseDisplayName(MethodInfo info, object[] data)
    {
        return data[0] + " / " + data[1];
    }

    private static ParserConfiguration CreateConfig(TestConfig config)
    {
        var con = config.Base switch
        {
            "twhl" => ParserConfiguration.Twhl("https://twhl.info"),
            "snarkpit" => ParserConfiguration.Snarkpit("https://snarkpit.net"),
            _ => new ParserConfiguration("https://example.com")
        };

        foreach (var e in config.Elements)
        {
            con.Elements.Add(e switch
            {
                "MdCode" => new MdCodeElement(),
                "MdColumns" => new MdColumnsElement(),
                "MdHeading" => new MdHeadingElement(),
                "MdLine" => new MdLineElement(),
                "MdList" => new MdListElement(),
                "MdPanel" => new MdPanelElement(),
                "MdQuote" => new MdQuoteElement(),
                "MdTable" => new MdTableElement(),
                "Pre" => new PreElement(),
                "Quote" => new QuoteElement(),
                "Ref" => new RefElement(),
                _ => throw new ArgumentException($"Unknown element name '{e}'")
            });
        }

        foreach (var t in config.Tags)
        {
            con.Tags.Add(t switch
            {
                "Strong" => new Tag("b", "strong").WithScopes("inline", "excerpt"),
                "Emphasis" => new Tag("i", "em").WithScopes("inline", "excerpt"),
                "Underline" => new Tag("u", "span", "underline").WithScopes("inline", "excerpt"),
                "Strikethrough" => new Tag("s", "span", "strikethrough").WithScopes("inline", "excerpt"),
                //
                "Align" => new AlignTag(),
                "Code" => new CodeTag(),
                "Color" => new ColorTag(),
                "Font" => new FontTag(),
                "Image" => new ImageTag(),
                "Link" => new LinkTag(),
                "List" => new ListTag(),
                "Pre" => new PreTag(),
                "QuickLink" => new QuickLinkTag(),
                "Quote" => new QuoteTag(),
                "Size" => new SizeTag(),
                "Spoiler" => new SpoilerTag(),
                "VaultEmbed" => new VaultEmbedTag(),
                "WikiArchive" => new WikiArchiveTag(),
                "WikiBook" => new WikiBookTag(),
                "WikiCategory" => new WikiCategoryTag(),
                "WikiCredit" => new WikiCreditTag(),
                "WikiFile" => new WikiFileTag(),
                "WikiImage" => new WikiImageTag(),
                "WikiLink" => new WikiLinkTag(),
                "WikiYoutube" => new WikiYoutubeTag(),
                "Youtube" => new YoutubeTag(),
                _ => throw new ArgumentException($"Unknown tag name '{t}'")
            });
        }

        foreach (var p in config.Processors)
        {
            con.Processors.Add(p switch
            {
                "AutoLinking" => new AutoLinkingProcessor(),
                "MarkdownText" => new MarkdownTextProcessor(),
                "NewLine" => new NewLineProcessor(),
                "Smilies" => new SmiliesProcessor("/smilies/{0}.png"),
                "TrimWhitespace" => new TrimWhitespaceAroundBlockNodesProcessor(),
                _ => throw new ArgumentException($"Unknown processor name '{p}'")
            });
        }

        return con;
    }

    private readonly static JsonSerializerOptions _jsonOptions = JsonSerializerOptions.Web;

    private readonly static IReadOnlyDictionary<string, string> _commonNames = new Dictionary<string, string>
    {
        { "list", nameof(NodeCollection) },
        { "plain", nameof(PlainTextNode) },
        { "html", nameof(HtmlNode) },
        { "meta", nameof(MetadataNode) },
        { "ref", nameof(RefNode) },
    };

    private static void AssertNodeHierarchy(TestNode expected, INode node, int lineOffset)
    {
        if (expected.Line > 0)
        {
            Assert.AreEqual(expected.Line + lineOffset, node.Line, $"Wrong line for '{expected.Name ?? expected.Type}'");
            Assert.AreEqual(expected.Column, node.Column, $"Wrong column for '{expected.Name ?? expected.Type}'");
        }

        var expectedType = _commonNames.GetValueOrDefault(expected.Type, node.GetType().Name);
        if (expectedType == nameof(UnprocessablePlainTextNode)) expectedType = nameof(PlainTextNode);

        var actualType = node.GetType().Name;
        if (actualType == nameof(UnprocessablePlainTextNode)) actualType = nameof(PlainTextNode);

        Assert.AreEqual(expectedType, actualType);

        var children = node.GetChildren();
        Assert.HasCount(expected.Children.Count, children);

        for (int i = 0; i < expected.Children.Count; i++)
        {
            AssertNodeHierarchy(expected.Children[i], children[i], lineOffset);
        }
    }

    [TestMethod]
    [DynamicData(nameof(GetTestCases), DynamicDataDisplayName = nameof(GetTestCaseDisplayName))]
    public void RunImplementationTest(string name, int lineOffset)
    {
        var dir = TestCaseUtils.GetTestCaseDirectory("implementations");
        var def = JsonSerializer.Deserialize<TestDefinition>(File.ReadAllText($"{dir}/{name}.json"), _jsonOptions)!;

        var input = def.Input;
        var config = CreateConfig(def.Config);
        var parser = new Parser(config);

        if (lineOffset > 0)
        {
            // for line offsets: add extra lines to the start of the input,
            // then add a custom element that eats those lines.
            // finally add a check to remove that element from the result.
            input = new string('\n', lineOffset + 1) + input;
            config.Elements.Insert(0, new LineOffsetEaterElement(lineOffset));
        }

        var result = parser.ParseResult(input);
        
        if (lineOffset > 0 && result.Content is NodeCollection nc && nc.Nodes.Count >= 2)
        {
            if (nc.Nodes[0] is UnprocessablePlainTextNode r1 && nc.Nodes[1] is UnprocessablePlainTextNode r2)
            {
                if (r1.Text == RemovedContentGuid && r2.Text == "\n") nc.Nodes.RemoveRange(0, 2);
            }
        }

        var resultHtml = result.ToHtml(TestCaseUtils.Sanitise);
        var resultPlain = result.ToPlainText();

        var expectedHtml = TestCaseUtils.Sanitise(def.Html);
        var expectedPlain = def.Plain;

        Assert.AreEqual(expectedHtml, resultHtml);
        Assert.AreEqual(expectedPlain, resultPlain);

        AssertNodeHierarchy(def.Nodes, result.Content, lineOffset);
    }

    private static readonly string RemovedContentGuid = Guid.NewGuid().ToString();

    private class LineOffsetEaterElement : Element
    {
        private readonly int _lineOffset;

        public LineOffsetEaterElement(int lineOffset)
        {
            _lineOffset = lineOffset;
            Priority = int.MaxValue;
        }

        public override bool Matches(Lines lines)
        {
            return _lineOffset > 0 && lines.SourceLine == 1;
        }

        public override INode Consume(Parser parser, ParseData data, Lines lines, string scope)
        {
            for (var i = 0; i < _lineOffset; i++) lines.Next();
            return new UnprocessablePlainTextNode(RemovedContentGuid);
        }
    }
}