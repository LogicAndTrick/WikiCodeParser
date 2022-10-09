using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LogicAndTrick.WikiCodeParser.Tags;

namespace LogicAndTrick.WikiCodeParser.Tests.Tags
{
    [TestClass]
    public class ImageTagTest : BaseTagTest<ImageTag>
    {
        protected override IEnumerable<TagTestData> GetTestData()
        {
            // Normal
            yield return new TagTestData("[img]https://example.com/example.png[/img]", @"<div class=""embedded image""><span class=""caption-panel""><img class=""caption-body"" src=""https://example.com/example.png"" alt=""User posted image"" /></span></div>", "\n[User posted image]\n") { IsBlock = true };
            yield return new TagTestData("[img url=https://example.com/example.png][/img]", @"<div class=""embedded image""><span class=""caption-panel""><img class=""caption-body"" src=""https://example.com/example.png"" alt=""User posted image"" /></span></div>", "\n[User posted image]\n") { IsBlock = true };
            yield return new TagTestData("[img url=example.com/example.png][/img]", @"<div class=""embedded image""><span class=""caption-panel""><img class=""caption-body"" src=""http://example.com/example.png"" alt=""User posted image"" /></span></div>", "\n[User posted image]\n") { IsBlock = true };
        }
    }
}
