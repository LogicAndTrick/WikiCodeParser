using System;
using WikiCodeParser.Nodes;

namespace WikiCodeParser.Elements
{
    public class PlainTextElement : BBCodeElement
    {
        public override bool Matches(Lines lines)
        {
            throw new NotImplementedException();
        }

        public override INode Consume(Parser parser, Lines lines)
        {
            throw new NotImplementedException();
        }
    }
}