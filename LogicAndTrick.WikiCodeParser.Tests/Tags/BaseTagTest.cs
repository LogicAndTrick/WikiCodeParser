using LogicAndTrick.WikiCodeParser.Tags;

namespace LogicAndTrick.WikiCodeParser.Tests.Tags;

public abstract class BaseTagTest<T> where T : Tag
{
    // ReSharper disable once VirtualMemberNeverOverridden.Global
    protected virtual Tag CreateTag()
    {
        return Activator.CreateInstance<T>();
    }

    protected abstract IEnumerable<TagTestData> GetTestData();

    // ReSharper disable once MemberCanBePrivate.Global
    protected Parser GetParser()
    {
        var config = new ParserConfiguration();
        config.Tags.Add(CreateTag());
        return new Parser(config);
    }

    // ReSharper disable once UnusedMember.Global
    protected ParseResult Parse(string input, string scope = "") => GetParser().ParseResult(input, scope);

    // ReSharper disable once MemberCanBePrivate.Global
    protected void TestSimpleTransform(TagTestData data, Func<string, string> transform)
    {
        var parser = GetParser();
        var input = transform(data.Input);
        var html = transform(data.ExpectedHtml);
        var plain = transform(data.ExpectedPlaintext);
        var result = parser.ParseResult(input);
        Assert.AreEqual(html, result.ToHtml());
        Assert.AreEqual(plain, result.ToPlainText());
        var expectedMeta = data.ExpectedMetaData;
        var meta = result.GetMetadata();
        Assert.AreEqual(expectedMeta.Count, meta.Count);
        for (var i = 0; i < meta.Count; i++)
        {
            var em = expectedMeta[i];
            var m = meta[i];
            Assert.AreEqual(em.Key, m.Key);
            Assert.AreEqual(em.Value, m.Value);
        }
    }

    // ReSharper disable once MemberCanBePrivate.Global
    protected void TestSimpleTransform(IEnumerable<TagTestData> data, Func<string, string> transform)
    {
        foreach (var d in data)
        {
            TestSimpleTransform(d, transform);
        }
    }

    [TestMethod]
    public void TestStandalone()
    {
        var data = GetTestData();
        TestSimpleTransform(data, x => x);
    }

    [TestMethod]
    public void TestBefore()
    {
        var data = GetTestData();
        TestSimpleTransform(data, x => $"before {x}");
    }

    [TestMethod]
    public void TestAfter()
    {
        var data = GetTestData();
        TestSimpleTransform(data, x => $"{x} after");
    }

    [TestMethod]
    public void TestBeforeAndAfter()
    {
        var data = GetTestData();
        TestSimpleTransform(data, x => $"before {x} after");
    }

    [TestMethod]
    public void TestNewLine()
    {
        var data = GetTestData();
        TestSimpleTransform(data, x => $"before\n{x}\nafter");
    }
}