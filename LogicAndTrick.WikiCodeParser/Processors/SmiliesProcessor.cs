using System;
using System.Collections.Generic;
using System.Linq;
using LogicAndTrick.WikiCodeParser.Nodes;

namespace LogicAndTrick.WikiCodeParser.Processors
{
    public class SmiliesProcessor : INodeProcessor
    {
        public int Priority { get; set; } = 5;

        public string UrlFormatString { get; set; } = "https://twhl.info/images/smilies/{0}.png";

        private readonly List<SmileyDefinition> _definitions;
        private bool _initialised;
        private char[] _tokenStarts;

        private const int MaxSmilies = 100;

        public SmiliesProcessor()
        {
            _definitions = new List<SmileyDefinition>();
        }

        public bool ShouldProcess(INode node, string scope)
        {
            return node is PlainTextNode && _definitions.Count > 0;
        }

        public IEnumerable<INode> Process(Parser parser, ParseData data, INode node, string scope)
        {
            if (!_initialised)
            {
                _tokenStarts = _definitions.SelectMany(x => x.Tokens).Select(x => x[0]).Distinct().ToArray();
                _initialised = true;
            }

            var text = ((PlainTextNode) node).Text;
            var start = 0;
            var index = -1;
            var numSmilies = 0;
            while (index + 1 < text.Length && (index = text.IndexOfAny(_tokenStarts, index + 1)) >= 0)
            {
                if (numSmilies > MaxSmilies)
                {
                    yield return new HtmlNode("<em class=\"text-danger\">", new UnprocessablePlainTextNode(" [warning: too many smilies in post] "), "</em>");
                    break;
                }

                // Must start with whitespace
                if (index != 0 && !Char.IsWhiteSpace(text[index - 1])) continue;

                // Find an appropriate definition
                SmileyDefinition definition = null;
                string token = null;
                foreach (var def in _definitions)
                {
                    token = def.GetMatchingToken(text, index);
                    if (token == null) continue;

                    definition = def;
                    break;
                }
                if (definition == null) continue;

                // Must end with whitespace
                if (index + token.Length < text.Length - 1 && !Char.IsWhiteSpace(text[index + token.Length])) continue;

                // We have a smiley
                if (start < index) yield return new PlainTextNode(text.Substring(start, index - start));
                var src = HtmlHelper.AttributeEncode(String.Format(UrlFormatString, definition.Name));
                var alt = HtmlHelper.AttributeEncode(token);
                yield return new HtmlNode($"<img class=\"smiley\" src=\"{src}\" alt=\"{alt}\" />", PlainTextNode.Empty, "") { PlainBefore = token };
                start = index + token.Length;
                index += token.Length;

                numSmilies++;
            }
            
            if (start < text.Length) yield return new PlainTextNode(text.Substring(start));
        }

        public SmiliesProcessor Add(string name, params string[] tokens)
        {
            _definitions.Add(new SmileyDefinition(name, tokens));
            _initialised = false;
            return this;
        }

        public SmiliesProcessor AddDefault()
        {
            Add("aggrieved"   , ":aggrieved:"              );
            Add("aghast"      , ":aghast:"                 );
            Add("angry"       , ":x", ":-x", ":angry:"     );
            Add("badass"      , ":badass:"                 );
            Add("confused"    , ":confused:"               );
            Add("cry"         , ":cry:"                    );
            Add("cyclops"     , ":cyclops:"                );
            Add("lol"         , ":lol:"                    );
            Add("frown"       , ":|", ":-|", ":frown:"     );
            Add("furious"     , ":furious:"                );
            Add("glad"        , ":glad:"                   );
            Add("heart"       , ":heart:"                  );
            Add("grin"        , ":D", ":-D", ":grin:"      );
            Add("nervous"     , ":nervous:"                );
            Add("nuke"        , ":nuke:"                   );
            Add("nuts"        , ":nuts:"                   );
            Add("quizzical"   , ":quizzical:"              );
            Add("rollseyes"   , ":roll:", ":rollseyes:"    );
            Add("sad"         , ":(", ":-(", ":sad:"       );
            Add("smile"       , ":)", ":-)", ":smile:"     );
            Add("surprised"   , ":o", ":-o", ":surprised:" );
            Add("thebox"      , ":thebox:"                 );
            Add("thefinger"   , ":thefinger:"              );
            Add("tired"       , ":tired:"                  );
            Add("tongue"      , ":P", ":-P", ":tongue:"    );
            Add("toocool"     , ":cool:"                   );
            Add("unsure"      , ":\\", ":-\\", ":unsure:"  );
            Add("biggrin"     , ":biggrin:"                );
            Add("wink"        , ";)", ";-)", ":wink:"      );
            Add("zonked"      , ":zonked:"                 );
            Add("sarcastic"   , ":sarcastic:"              );
            Add("combine"     , ":combine:", ":elite:"     );
            Add("gak"         , ":gak:"                    );
            Add("animehappy"  , ":^_^:"                    );
            Add("pwnt"        , ":pwned:"                  );
            Add("target"      , ":target:"                 );
            Add("ninja"       , ":ninja:"                  );
            Add("hammer"      , ":hammer:"                 );
            Add("pirate"      , ":pirate:", ":yar:"        );
            Add("walter"      , ":walter:"                 );
            Add("plastered"   , ":plastered:"              );
            Add("bigmouth"    , ":zomg:"                   );
            Add("brokenheart" , ":heartbreak:"             );
            Add("ciggiesmilie", ":ciggie:"                 );
            Add("combines"    , ":combines:"               );
            Add("crowbar"     , ":crowbar:"                );
            Add("death"       , ":death:"                  );
            Add("freeman"     , ":freeman:"                );
            Add("hecu"        , ":hecu:"                   );
            Add("nya"         , ":nya:"                    );
            return this;
        }

        private class SmileyDefinition
        {
            public string Name { get; set; }
            public string[] Tokens { get; set; }

            public SmileyDefinition(string name, params string[] tokens)
            {
                Name = name;
                Tokens = tokens;
            }

            public string GetMatchingToken(string text, int startIndex)
            {
                foreach (var token in Tokens)
                {
                    if (text.IndexOf(token, startIndex, StringComparison.Ordinal) == startIndex) return token;
                }

                return null;
            }
        }
    }
}
