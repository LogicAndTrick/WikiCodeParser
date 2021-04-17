using System;
using WikiCodeParser.Elements;
using WikiCodeParser.Tags;

namespace WikiCodeParser.Sandbox
{
    class Program
    {
        static void Main(string[] args)
        {
            var parser = new Parser();
            parser.Elements.Add(new MdCodeElement());
            parser.Tags.Add(new WikiCategoryTag());
            var result = parser.ParseResult(@"[cat:Tutorials]
[book:Half-Life Programming|chapter:Monster Programming|chapternumber:4|pagenumber:1]
[credit:Original author|user:5504]
```c
	Hello
		This is some code
```
	");
            var meta = result.Content.GetMeta();
            var plain = result.ToPlainText();
            var html = result.ToHtml();

            Console.WriteLine("Meta:");
            foreach (var m in meta)
            {
                foreach (var v in m.Value) Console.WriteLine($"{m.Key}: {v}");
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
