﻿using System;

namespace LogicAndTrick.WikiCodeParser.Sandbox
{
    class Program
    {
        static void Main(string[] args)
        {
            var parser = new Parser(ParserConfiguration.Twhl());
            var result = parser.ParseResult(@":)");
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
