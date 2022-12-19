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

        public SmiliesProcessor AddTwhl()
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

        public SmiliesProcessor AddSnarkpit()
        {
            Add("icon_biggrin"    , ":D");
            Add("sailor"          , ":sailor:"      );
            Add("icon_smile"      , ":)", ":-)"     );
            Add("dorky"           , ":geek:"        );
            Add("sad0019"         , ":("            );
            Add("icon_eek"        , ":-o"           );
            Add("grenade"         , ":grenade:"     );
            Add("confused"        , ":confused:"    );
            Add("icon_cool"       , "-)"            );
            Add("kitty"           , "k1tt3h:"       );
            Add("laughing"        , ":lol:"         );
            Add("leper"           , ":leper:"       );
            Add("mad"             , ":mad:"         );
            Add("tongue0010"      , ":p"            );
            Add("popcorn"         , ":popcorn:"     );
            Add("icon_redface"    , ":oops:"        );
            Add("icon_cry"        , ":cry:"         );
            Add("icon_twisted"    , ":evil:"        );
            Add("rolleye0011"     , ":roll:"        );
            Add("shocked"         , ":scream:"      );
            Add("icon_wink"       , "];)"           );
            Add("dead"            , ":dead:"        );
            Add("pimp"            , ":pimp:"        );
            Add("beerchug"        , ":beer:"        );
            Add("chainsaw"        , ":chainsaw:"    );
            Add("arse"            , ":moonie:"      );
            Add("angel"           , ":angel:"       );
            Add("bday"            , ":bday:"        );
            Add("clap"            , ":clap:"        );
            Add("computer"        , ":computer:"    );
            Add("crash"           , ":pccrash:"     );
            Add("dizzy"           , ":dizzy:"       );
            Add("dodgy"           , ":naughty:"     );
            Add("drink"           , ":drink:"       );
            Add("facelick"        , ":lick:"        );
            Add("frown"           , ">:("           );
            Add("heee"            , ":hee:"         );
            Add("imwithstupid"    , ":imwithstupid:");
            Add("jawdrop"         , ":jawdrop:"     );
            Add("king"            , ":king:"        );
            Add("ladysman"        , ":ladysman:"    );
            Add("mrT"             , ":mrt:"         );
            Add("nurse"           , ":nurse:"       );
            Add("outtahere"       , ":outtahere:"   );
            Add("aaatrigger"      , ":aaatrigger:"  );
            Add("repuke"          , ":repuke:"      );
            Add("rofl"            , ":rofl:"        );
            Add("rolling"         , ":rolling2:"    );
            Add("santa"           , ":santa:"       );
            Add("smash"           , ":smash:"       );
            Add("toilet"          , ":toilet:"      );
            Add("44"              , "~o)"           );
            Add("wavey"           , ":wavey:"       );
            Add("upyours"         , ":stfu:"        );
            Add("fart"            , ":fart:"        );
            Add("trout"           , ":trout:"       );
            Add("ar15firing"      , ":machinegun:"  );
            Add("microwave"       , ":microwave:"   );
            Add("guillotine"      , ":guillotine:"  );
            Add("poke"            , ":poke:"        );
            Add("sniper"          , ":sniper:"      );
            Add("monkee"          , ":monkee:"      );
            Add("bandit"          , ":gringo:"      );
            Add("wtf"             , ":wtf:"         );
            Add("azelito"         , ":azelito:"     );
            Add("crate"           , ":crate:"       );
            Add("argh"            , ":-&"           );
            Add("swear"           , ":swear:"       );
            Add("rocketwhore"     , ":launcher:"    );
            Add("skull"           , ":skull:"       );
            Add("munky"           , ":munky:"       );
            Add("evilgrin"        , ":E"            );
            Add("banghead"        , ":brickwall:"   );
            Add("wcc"             , ":wcc:"         );
            Add("smiley_sherlock" , ":sherlock:"    );
            Add("nag"             , ":nag:"         );
            Add("rolling_eyes"    , ":rolling:"     );
            Add("angryfire"       , ":flame:"       );
            Add("character"       , ":ghost:"       );
            Add("character0007"   , ":pirate:"      );
            Add("indifferent0016" , ":zzz:"         );
            Add("indifferent0002" , ":|"            );
            Add("love0012"        , ":love:"        );
            Add("rolleye0006"     , ":lookup:"      );
            Add("sad0006"         , "];("           );
            Add("scared0005"      , ":scared:"      );
            Add("flail"           , ":flail:"       );
            Add("emot-cowjump"    , ":cowjump:"     );
            Add("emot-eng101"     , ":teach:"       );
            Add("uncertain"       , ":uncertain:"   );
            Add("1sm071potstir"   , ":stirring:"    );
            Add("thumbs_up"       , ":thumbsup:"    );
            Add("happy_open"      , ":happy:"       );
            Add("snark_topic_icon", ":snark:"       );
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
