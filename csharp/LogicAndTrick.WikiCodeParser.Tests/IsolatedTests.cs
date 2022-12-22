namespace LogicAndTrick.WikiCodeParser.Tests;

[TestClass]
public class IsolatedTests
{
    [DataTestMethod]
    [DataRow("missing-tag")]
    [DataRow("unicode-escape")]
    [DataRow("ref-simple")]
    [DataRow("pre-simple")]
    [DataRow("pre-lang")]
    [DataRow("pre-highlight")]
    [DataRow("mdcode-simple")]
    [DataRow("mdcode-lang")]
    [DataRow("heading-simple")]
    [DataRow("mdline-simple")]
    [DataRow("columns-simple")]
    [DataRow("columns-invalid")]
    [DataRow("panel-simple")]
    [DataRow("mdquote-simple")]
    [DataRow("mdquote-nested")]
    [DataRow("list-simple")]
    [DataRow("list-nested")]
    [DataRow("list-continuation")]
    [DataRow("table-simple")]
    [DataRow("table-ref")]
    [DataRow("tags-plain")]
    [DataRow("code-tag")]
    [DataRow("font-tag")]
    [DataRow("h-tag")]
    [DataRow("pre-tag")]
    [DataRow("quote-tag")]
    [DataRow("image-tag")]
    [DataRow("link-tag")]
    [DataRow("vault-tag")]
    [DataRow("quick-link-tag")]
    [DataRow("spoiler-tag")]
    [DataRow("youtube-tag")]
    [DataRow("wiki-category-tag")]
    [DataRow("wiki-image-tag")]
    [DataRow("wiki-file-tag")]
    [DataRow("wiki-credit-tag")]
    [DataRow("wiki-book-tag")]
    [DataRow("wiki-archive-tag")]
    [DataRow("wiki-youtube-tag")]
    [DataRow("wiki-link-tag")]
    [DataRow("processor-newlines")]
    [DataRow("processor-smilies-basic")]
    [DataRow("processor-smilies-toomany")]
    [DataRow("processor-markdowntext")]
    [DataRow("processor-autolinking")]
    public void IsolatedTest(string name)
    {
        TestCaseUtils.RunTestCase(ParserConfiguration.Twhl(), "isolated", name);
    }

    [DataTestMethod]
    [DataRow("pre-simple")]
    [DataRow("pre-lang")]
    [DataRow("pre-highlight")]
    [DataRow("code-tag")]
    [DataRow("pre-tag")]
    [DataRow("align-tag")]
    [DataRow("size-tag")]
    [DataRow("color-tag")]
    [DataRow("list-tag")]
    public void IsolatedTestSnarkpit(string name)
    {
        TestCaseUtils.RunTestCase(ParserConfiguration.Snarkpit(), "isolated-sp", name);
    }
}