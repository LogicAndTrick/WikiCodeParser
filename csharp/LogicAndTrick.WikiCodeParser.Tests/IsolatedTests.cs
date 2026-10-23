namespace LogicAndTrick.WikiCodeParser.Tests;

[TestClass]
public class IsolatedTests
{
    public static IEnumerable<string> GetTestCases()
    {
        return TestCaseUtils.GetTestCaseNames("isolated");
    }

    [TestMethod]
    [DynamicData(nameof(GetTestCases))]
    public void IsolatedTest(string name)
    {
        TestCaseUtils.RunTestCase(ParserConfiguration.Twhl(), "isolated", name);
    }

    public static IEnumerable<string> GetSnarkpitTestCases()
    {
        return TestCaseUtils.GetTestCaseNames("isolated-sp");
    }
    
    [DataTestMethod]
    [TestMethod]
    [DynamicData(nameof(GetSnarkpitTestCases))]
    public void IsolatedTestSnarkpit(string name)
    {
        TestCaseUtils.RunTestCase(ParserConfiguration.Snarkpit(), "isolated-sp", name);
    }
}