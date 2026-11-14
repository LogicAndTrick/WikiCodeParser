
using System.Text.Json.Nodes;

namespace LogicAndTrick.WikiCodeParser.Tests.TestModels;

public class TestNode
{
    public string? Name { get; set; }
    public required string Type { get; set; }
    public required int[] Pos { get; set; } = [];
    public List<TestNode> Children { get; set; } = [];
    public string? Text { get; set; }
    public string[] Html { get; set; } = [];
    public string[] Plain { get; set; } = [];
    public string? Key { get; set; }
    public JsonNode? Value { get; set; }

    public int Line => Pos.Length == 2 ? Pos[0] : 0;
    public int Column => Pos.Length == 2 ? Pos[1] : 0;
}
