
namespace LogicAndTrick.WikiCodeParser.Tests.TestModels;

public class TestDefinition
{
    public required TestConfig Config { get; set; }
    public required string Input { get; set; }
    public required string Html { get; set; }
    public required string Plain { get; set; }
    public required TestNode Nodes { get; set; }
}
