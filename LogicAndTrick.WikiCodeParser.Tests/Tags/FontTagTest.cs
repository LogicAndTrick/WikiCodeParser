using LogicAndTrick.WikiCodeParser.Tags;

namespace LogicAndTrick.WikiCodeParser.Tests.Tags;

[TestClass]
public class FontTagTest : BaseTagTest<FontTag>
{
    protected override IEnumerable<TagTestData> GetTestData()
    {
        yield return new TagTestData("[font]font tag[/font]", "<span>font tag</span>", "font tag");

        // colours
        yield return new TagTestData("[font=red]font tag[/font]", "<span style=\"color: red;\">font tag</span>", "font tag");
        yield return new TagTestData("[font=#123]font tag[/font]", "<span style=\"color: #123;\">font tag</span>", "font tag");
        yield return new TagTestData("[font=#123456]font tag[/font]", "<span style=\"color: #123456;\">font tag</span>", "font tag");
        yield return new TagTestData("[font color=red]font tag[/font]", "<span style=\"color: red;\">font tag</span>", "font tag");
        yield return new TagTestData("[font color=#123]font tag[/font]", "<span style=\"color: #123;\">font tag</span>", "font tag");
        yield return new TagTestData("[font color=#123456]font tag[/font]", "<span style=\"color: #123456;\">font tag</span>", "font tag");
        yield return new TagTestData("[font colour=red]font tag[/font]", "<span style=\"color: red;\">font tag</span>", "font tag");
        yield return new TagTestData("[font colour=#123]font tag[/font]", "<span style=\"color: #123;\">font tag</span>", "font tag");
        yield return new TagTestData("[font colour=#123456]font tag[/font]", "<span style=\"color: #123456;\">font tag</span>", "font tag");

        // sizes
        yield return new TagTestData("[font size=12]font tag[/font]", "<span style=\"font-size: 12px;\">font tag</span>", "font tag");

        // both
        yield return new TagTestData("[font size=12 color=red]font tag[/font]", "<span style=\"color: red; font-size: 12px;\">font tag</span>", "font tag");
        yield return new TagTestData("[font size=14 color=red]font tag[/font]", "<span style=\"color: red; font-size: 14px;\">font tag</span>", "font tag");
        yield return new TagTestData("[font size=16 color=red]font tag[/font]", "<span style=\"color: red; font-size: 16px;\">font tag</span>", "font tag");
        yield return new TagTestData("[font size=12 color=#123]font tag[/font]", "<span style=\"color: #123; font-size: 12px;\">font tag</span>", "font tag");
        yield return new TagTestData("[font size=12 color=#123456]font tag[/font]", "<span style=\"color: #123456; font-size: 12px;\">font tag</span>", "font tag");

        // invalid colours
        yield return new TagTestData("[font=notacolor]font tag[/font]", "<span style=\"\">font tag</span>", "font tag");
        yield return new TagTestData("[font=#1234]font tag[/font]", "<span style=\"\">font tag</span>", "font tag");
        
        // invalid sizes
        yield return new TagTestData("[font size=1]font tag[/font]", "<span style=\"\">font tag</span>", "font tag");
        yield return new TagTestData("[font size=100]font tag[/font]", "<span style=\"\">font tag</span>", "font tag");
        yield return new TagTestData("[font size=aaa]font tag[/font]", "<span style=\"\">font tag</span>", "font tag");
    }
}