using System;
using System.Collections.Generic;
using System.Linq;
using WikiCodeParser.Nodes;

namespace WikiCodeParser.Elements
{
    public abstract class BBCodeElement
    {
        public List<string> Scopes { get; set; }
        public int Priority { get; set; } = 0;

        public virtual bool InScope(string scope) => string.IsNullOrWhiteSpace(scope) ||
                                                     Scopes.Contains(scope, StringComparer.InvariantCultureIgnoreCase);

        public abstract bool Matches(Lines lines);
        public abstract BBCodeContent Consume(Parser parser, Lines lines);
    }
}