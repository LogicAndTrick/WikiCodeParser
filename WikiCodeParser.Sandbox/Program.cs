using System;

namespace WikiCodeParser.Sandbox
{
    class Program
    {
        static void Main(string[] args)
        {
            var parser = new Parser(ParserConfiguration.Default);
            var result = parser.ParseResult(@"[cat:Tutorials]
[book:Half-Life Programming|chapter:Monster Programming|chapternumber:4|pagenumber:1]
[credit:Original author|user:5504]
```c
	Hello
		This is some code
```
test1 [code]test2[/code] test3
	");
            var meta = result.GetMetadata();
            var plain = result.ToPlainText();
            var html = result.ToHtml();

            Console.WriteLine("Meta:");
            foreach (var m in meta)
            {
                Console.WriteLine($"{m.Key}: {m.Value}");
            }
            Console.WriteLine("-------\n");
            Console.WriteLine("Plain:");
            Console.WriteLine(plain);
            Console.WriteLine("-------\n");
            Console.WriteLine("Html:");
            Console.WriteLine(html);
        }
    }
}
