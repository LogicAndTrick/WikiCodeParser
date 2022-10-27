namespace LogicAndTrick.WikiCodeParser.Tests;

[TestClass]
public class EndToEndTests
{
    [DataTestMethod]
    [DataRow("wikicode-page")]
    public void EndToEndTest(string name)
    {
        TestCaseUtils.RunTestCase("endtoend", name);
    }
}