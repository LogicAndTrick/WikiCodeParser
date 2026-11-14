
namespace LogicAndTrick.WikiCodeParser.Tests.TestModels;

public class TestDefinition
{
    public required TestConfig Config { get; set; }
    public required string Input { get; set; }
    public required string Html { get; set; }
    public required string Plain { get; set; }
    public required TestNode Nodes { get; set; }
}

public class TestNode
{
    public string? Name { get; set; }
    public required string Type { get; set; }
    public required int[] Pos { get; set; } = [];
    public List<TestNode> Children { get; set; } = [];
    public int Line => Pos.Length == 2 ? Pos[0] : 0;
    public int Column => Pos.Length == 2 ? Pos[1] : 0;
}

public class TestConfig
{
    public string Base { get; set; } = "none";
    public List<string> Elements { get; set; } = [];
    public List<string> Tags { get; set; } = [];
    public List<string> Processors { get; set; } = [];
}
