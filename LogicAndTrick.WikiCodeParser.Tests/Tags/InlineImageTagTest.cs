using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LogicAndTrick.WikiCodeParser.Tags;

namespace LogicAndTrick.WikiCodeParser.Tests.Tags
{
    [TestClass]
    public class InlineImageTagTest : BaseTagTest<ImageTag>
    {
        protected override Tag CreateTag()
        {
            return new ImageTag().WithToken("simg").WithBlock(false);
        }

        protected override IEnumerable<TagTestData> GetTestData()
        {
            // Normal
            yield return new TagTestData("[simg]https://example.com/example.png[/simg]", @"<span class=""embedded image inline""><span class=""caption-panel""><img class=""caption-body"" src=""https://example.com/example.png"" alt=""User posted image"" /></span></span>", "[User posted image]");
            yield return new TagTestData("[simg url=https://example.com/example.png][/simg]", @"<span class=""embedded image inline""><span class=""caption-panel""><img class=""caption-body"" src=""https://example.com/example.png"" alt=""User posted image"" /></span></span>", "[User posted image]");
            yield return new TagTestData("[simg url=example.com/example.png][/simg]", @"<span class=""embedded image inline""><span class=""caption-panel""><img class=""caption-body"" src=""http://example.com/example.png"" alt=""User posted image"" /></span></span>", "[User posted image]");
        }
    }
}
