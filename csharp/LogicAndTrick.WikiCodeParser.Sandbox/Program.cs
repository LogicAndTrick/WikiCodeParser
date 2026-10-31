using System;
using System.IO;
using System.Linq;
using Ganss.Xss;

namespace LogicAndTrick.WikiCodeParser.Sandbox
{
    class Program
    {
        static HtmlSanitizer sanitiser = new HtmlSanitizer
        {
            AllowDataAttributes = true,
            AllowedTags = { "video", "audio" },
            AllowedAttributes = { "id", "class", "playsinline", "controls", "autoplay", "loop", "muted" },
            AllowedSchemes = { "ftp", "mailto" }
        };

        static string Sanitise(string html)
        {
            // sanitiser.RemovingAttribute += (_, args) => Console.WriteLine($"Removing attr: {args.Tag.TagName}/{args.Attribute.Name}/{args.Attribute.Value}: {args.Reason}");
            // sanitiser.RemovingStyle += (_, args) => Console.WriteLine($"Removing style: {args.Tag.TagName}/{args.Style.Name}: {args.Reason}");
            // sanitiser.RemovingTag += (_, args) => Console.WriteLine($"Removing tag: {args.Tag.TagName}: {args.Reason}");
            return sanitiser.Sanitize(html, "");
        }

        static void Main(string[] args)
        {
            var parser = new Parser(ParserConfiguration.Twhl());
            var str = ":)";
            var result = parser.ParseResult(str);
            var meta = result.GetMetadata();
            var plain = result.ToPlainText();
            var html = result.ToHtml(Sanitise);

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
