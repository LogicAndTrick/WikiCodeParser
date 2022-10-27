using LogicAndTrick.WikiCodeParser.Tags;

namespace LogicAndTrick.WikiCodeParser.Tests.Tags;

[TestClass]
public class CodeTagTest : BaseTagTest<CodeTag>
{
    protected override IEnumerable<TagTestData> GetTestData()
    {
        // Normal
        yield return new TagTestData("[code]code?[/code]", "<code>code?</code>", "code?");

        // Nested (shouldn't work)
        yield return new TagTestData("[code][code]code?[/code][/code]", "<code>[code]code?</code>[/code]", "[code]code?[/code]");
    }
}