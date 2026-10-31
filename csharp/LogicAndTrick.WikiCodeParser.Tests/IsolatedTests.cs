namespace LogicAndTrick.WikiCodeParser.Tests;

[TestClass]
public class IsolatedTests
{
    public static IEnumerable<string> GetTestCases()
    {
        return TestCaseUtils.GetTestCaseNames("isolated");
    }

    [TestMethod]
    [Timeout(1000)]
    [DynamicData(nameof(GetTestCases))]
    public void IsolatedTest(string name)
    {
        TestCaseUtils.RunTestCase(ParserConfiguration.Twhl(), "isolated", name);
    }

    public static IEnumerable<string> GetSnarkpitTestCases()
    {
        return TestCaseUtils.GetTestCaseNames("isolated-sp");
    }
    
    [TestMethod]
    [Timeout(1000)]
    [DynamicData(nameof(GetSnarkpitTestCases))]
    public void IsolatedTestSnarkpit(string name)
    {
        TestCaseUtils.RunTestCase(ParserConfiguration.Snarkpit(), "isolated-sp", name);
    }
}