using LogicAndTrick.WikiCodeParser.Tags;

namespace LogicAndTrick.WikiCodeParser.Tests.Tags;

[TestClass]
public class QuoteTagTest : BaseTagTest<QuoteTag>
{
    protected override IEnumerable<TagTestData> GetTestData()
    {
        // Normal
        yield return new TagTestData("[quote]quote[/quote]", "<blockquote>quote</blockquote>", "[quote]\nquote\n[/quote]");

        // Nested
        yield return new TagTestData("[quote][quote]quote[/quote][/quote]", "<blockquote><blockquote>quote</blockquote></blockquote>", "[quote]\n[quote]\nquote\n[/quote]\n[/quote]");

        // Case insensitivity
        yield return new TagTestData("[quote][QUOTE]quote[/quote][/QUOTE]", "<blockquote><blockquote>quote</blockquote></blockquote>", "[quote]\n[quote]\nquote\n[/quote]\n[/quote]");
    }
}