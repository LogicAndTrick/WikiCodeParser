using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LogicAndTrick.WikiCodeParser.Tests;

public static class TestCaseUtils
{
    private static void AssertSame(string name, string expected, string actual, bool split)
    {
        if (split)
        {
            var expectedLines = expected.Split('\n');
            var actualLines = actual.Split('\n');

            for (var i = 0; i < expectedLines.Length; i++)
            {
                var ex = expectedLines[i];
                var ac = actualLines[i];
                Assert.AreEqual(ex, ac, $"[{name}] \n\nMatch failed on line {i + 1}.\nExpected: {ex}\nActual  : {ac}");
            }
        }
        else
        {
            Assert.AreEqual(expected, actual, $"[{name}] Match failed.");
        }
    }

    private static void Test(ParserConfiguration config, string input, string expectedOutput, string? expectedPlain, string? expectedMeta, bool split = false)
    {
        var parser = new Parser(config);

        var result = parser.ParseResult(input);
        var resultHtml = result.ToHtml().Trim();
        var resultPlain = result.ToPlainText().Trim();

        AssertSame("html", expectedOutput, resultHtml, split);
        if (expectedPlain != null) AssertSame("plain", expectedPlain, resultPlain, split);


        if (expectedMeta != null)
        {
            var resultMetaObjects = result.GetMetadata().Select(x => new { x.Key, Value = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(x.Value)) }).ToList();
            var expectedMetaObjects = expectedMeta.Split('\n').Select(x => x.Trim().Split('=', 2)).Select(x => new { Key = x[0], Value = JsonConvert.DeserializeObject(x[1]) }).ToList();
            Assert.AreEqual(expectedMetaObjects.Count, resultMetaObjects.Count);
            for (var i = 0; i < resultMetaObjects.Count; i++)
            {
                var rmo = resultMetaObjects[i];
                var emo = expectedMetaObjects[i];
                if (emo.Value.ToString() == rmo.Value.ToString()) continue;
                var robj = (JObject)rmo.Value;
                var eobj = (JObject)rmo.Value;
                foreach (var ep in eobj.Properties())
                {
                    var rp = robj.Property(ep.Name);
                    var ev = ep.Value?.ToString();
                    var rv = rp.Value?.ToString();
                    Assert.AreEqual(ev, rv);
                }
            }
        }
    }

    public static string GetTestCaseDirectory(string folder)
    {
        if (Environment.GetEnvironmentVariable("VSTEST_BACKGROUND_DISCOVERY") == "1")
        {
            // Live unit testing
            // I don't know a better way, so for now we hard-code
            return Path.Combine(@"D:\Github\WikiCodeParser\tests", folder);
        }

        // Walk up the folder tree until we hit the repo root
        var dir = Environment.CurrentDirectory;
        while (dir != null && !Directory.Exists(Path.Combine(dir, "tests", folder)))
        {
            dir = Path.GetDirectoryName(dir);
        }

        if (dir == null) throw new FileNotFoundException("Cannot find test case folder", folder);

        return Path.Combine(dir, "tests", folder);
    }

    // ReSharper disable InconsistentNaming
    public static void RunTestCase(ParserConfiguration config, string folder, string name, bool split = false)
    {
        var dir = GetTestCaseDirectory(folder);
        string _in;
        string _out;
        string? _plain = null;
        string? _meta = null;
        if (File.Exists($"{dir}/{name}"))
        {
            var text = File.ReadAllText($"{dir}/{name}");
            var spl = text.Split("###").Select(x => x.Trim()).ToArray();
            _in = spl[0];
            _out = spl[1];
            if (spl.Length > 2) _plain = spl[2];
            if (spl.Length > 3) _meta = spl[3];
        }
        else
        {
            _in = File.ReadAllText($"{dir}/{name}.in");
            _out = File.ReadAllText($"{dir}/{name}.out");
            if (File.Exists($"{dir}/{name}.plain"))
            {
                _plain = File.ReadAllText($"{dir}/{name}.plain");
            }
            if (File.Exists($"{dir}/{name}.meta"))
            {
                _meta = File.ReadAllText($"{dir}/{name}.meta");
            }
        }

        _in = _in.Replace("\r", "");
        _out = _out.Replace("\r", "");
        _plain = _plain?.Replace("\r", "");
        _meta = _meta?.Replace("\r", "");
        Test(config, _in, _out, _plain, _meta, split);
    }
    // ReSharper restore InconsistentNaming
}