<?php

namespace LogicAndTrick\WikiCodeParser\Processors;

use LogicAndTrick\WikiCodeParser\HtmlHelper;
use LogicAndTrick\WikiCodeParser\Nodes\HtmlNode;
use LogicAndTrick\WikiCodeParser\Nodes\INode;
use LogicAndTrick\WikiCodeParser\Nodes\PlainTextNode;
use LogicAndTrick\WikiCodeParser\Nodes\UnprocessablePlainTextNode;
use LogicAndTrick\WikiCodeParser\ParseData;
use LogicAndTrick\WikiCodeParser\Parser;
use LogicAndTrick\WikiCodeParser\Util;

class SmileyDefinition
{
    public string $name;
    /**
     * @var string[]
     */
    public array $tokens;

    public function __construct(string $name, string ...$tokens)
    {
        $this->name = $name;
        $this->tokens = $tokens;
    }

    public function GetMatchingToken(string $text, int $startIndex) : ?string {
        foreach ($this->tokens as $token) {
            if (strpos($text, $token, $startIndex) === $startIndex) return $token;
        }
        return null;
    }
}

class SmiliesProcessor implements INodeProcessor
{
    public string $urlFormatString;

    /**
     * @var SmileyDefinition[]
     */
    private array $definitions;
    private bool $initialised;
    /**
     * @var string[]
     */
    private array $tokenStarts;

    private static int $MaxSmilies = 100;

    public function __construct(string $urlFormatString) {
        $this->urlFormatString = $urlFormatString;
        $this->definitions = [];
        $this->initialised = false;
    }

    function Priority(): int
    {
        return 5;
    }

    function ShouldProcess(INode $node, string $scope): bool
    {
        return $node instanceof PlainTextNode && count($this->definitions) > 0;
    }

    function Process(Parser $parser, ParseData $data, INode $node, string $scope): array
    {
        if (!$this->initialised) {
            $this->tokenStarts = array_unique(array_map(fn(string $x) => $x[0], array_merge(...array_map(fn(SmileyDefinition $def) => $def->tokens, $this->definitions))));
            $this->initialised = true;
        }

        $ret = [];

        /** @var PlainTextNode $node */
        $text = $node->text;
        $start = 0;
        $index = -1;
        $numSmilies = 0;
        while ($index + 1 < strlen($text) && ($index = Util::IndexOfAny($text, $this->tokenStarts, $index + 1)) >= 0) {
            if ($numSmilies > self::$MaxSmilies) {
                $ret[] = new HtmlNode('<em class="text-danger">', new UnprocessablePlainTextNode(' [warning: too many smilies in post] '), '</em>');
                break;
            }

            // Must start with whitespace
            if ($index != 0 && trim($text[$index - 1]) != '') continue;

            // Find an appropriate definition
            /** @var ?SmileyDefinition $definition */
            $definition = null;
            /** @var ?string $token */
            $token = null;
            foreach ($this->definitions as $def) {
                $token = $def->GetMatchingToken($text, $index);
                if ($token == null) continue;

                $definition = $def;
                break;
            }
            if ($definition == null) continue;

            // Must end with whitespace
            if ($index + strlen($token) < strlen($text) - 1 && trim($text[$index + strlen($token)]) != '') continue;

            // We have a smiley
            if ($start < $index) $ret[] = new PlainTextNode(substr($text, $start, $index - $start));
            $src = HtmlHelper::AttributeEncode(Util::Template($this->urlFormatString, [ '0' => $definition->name ]));
            $alt = HtmlHelper::AttributeEncode($token);
            $node = new HtmlNode("<img class=\"smiley\" src=\"${src}\" alt=\"${alt}\" />", PlainTextNode::Empty(), '');
            $node->plainBefore = $token;
            $ret[] = $node;
            $start = $index + strlen($token);
            $index += strlen($token);

            $numSmilies++;
        }

        if ($start < strlen($text)) $ret[] = new PlainTextNode(substr($text, $start));
        return $ret;
    }

    public function Add(string $name, string ...$tokens): SmiliesProcessor {
        $this->definitions[] = new SmileyDefinition($name, ...$tokens);
        $this->initialised = false;
        return $this;
    }

    public function AddTwhl() : SmiliesProcessor
    {
        $this->Add('aggrieved'   , ':aggrieved:'              );
        $this->Add('aghast'      , ':aghast:'                 );
        $this->Add('angry'       , ':x', ':-x', ':angry:'     );
        $this->Add('badass'      , ':badass:'                 );
        $this->Add('confused'    , ':confused:'               );
        $this->Add('cry'         , ':cry:'                    );
        $this->Add('cyclops'     , ':cyclops:'                );
        $this->Add('lol'         , ':lol:'                    );
        $this->Add('frown'       , ':|', ':-|', ':frown:'     );
        $this->Add('furious'     , ':furious:'                );
        $this->Add('glad'        , ':glad:'                   );
        $this->Add('heart'       , ':heart:'                  );
        $this->Add('grin'        , ':D', ':-D', ':grin:'      );
        $this->Add('nervous'     , ':nervous:'                );
        $this->Add('nuke'        , ':nuke:'                   );
        $this->Add('nuts'        , ':nuts:'                   );
        $this->Add('quizzical'   , ':quizzical:'              );
        $this->Add('rollseyes'   , ':roll:', ':rollseyes:'    );
        $this->Add('sad'         , ':(', ':-(', ':sad:'       );
        $this->Add('smile'       , ':)', ':-)', ':smile:'     );
        $this->Add('surprised'   , ':o', ':-o', ':surprised:' );
        $this->Add('thebox'      , ':thebox:'                 );
        $this->Add('thefinger'   , ':thefinger:'              );
        $this->Add('tired'       , ':tired:'                  );
        $this->Add('tongue'      , ':P', ':-P', ':tongue:'    );
        $this->Add('toocool'     , ':cool:'                   );
        $this->Add('unsure'      , ':\\', ':-\\', ':unsure:'  );
        $this->Add('biggrin'     , ':biggrin:'                );
        $this->Add('wink'        , ';)', ';-)', ':wink:'      );
        $this->Add('zonked'      , ':zonked:'                 );
        $this->Add('sarcastic'   , ':sarcastic:'              );
        $this->Add('combine'     , ':combine:', ':elite:'     );
        $this->Add('gak'         , ':gak:'                    );
        $this->Add('animehappy'  , ':^_^:'                    );
        $this->Add('pwnt'        , ':pwned:'                  );
        $this->Add('target'      , ':target:'                 );
        $this->Add('ninja'       , ':ninja:'                  );
        $this->Add('hammer'      , ':hammer:'                 );
        $this->Add('pirate'      , ':pirate:', ':yar:'        );
        $this->Add('walter'      , ':walter:'                 );
        $this->Add('plastered'   , ':plastered:'              );
        $this->Add('bigmouth'    , ':zomg:'                   );
        $this->Add('brokenheart' , ':heartbreak:'             );
        $this->Add('ciggiesmilie', ':ciggie:'                 );
        $this->Add('combines'    , ':combines:'               );
        $this->Add('crowbar'     , ':crowbar:'                );
        $this->Add('death'       , ':death:'                  );
        $this->Add('freeman'     , ':freeman:'                );
        $this->Add('hecu'        , ':hecu:'                   );
        $this->Add('nya'         , ':nya:'                    );
        return $this;
    }

    public function AddSnarkpit() : SmiliesProcessor
    {
        $this->Add('icon_biggrin'    , ':D'            );
        $this->Add('sailor'          , ':sailor:'      );
        $this->Add('icon_smile'      , ':)', ':-)'     );
        $this->Add('dorky'           , ':geek:'        );
        $this->Add('sad0019'         , ':('            );
        $this->Add('icon_eek'        , ':-o'           );
        $this->Add('grenade'         , ':grenade:'     );
        $this->Add('confused'        , ':confused:'    );
        $this->Add('icon_cool'       , '-)'            );
        $this->Add('kitty'           , 'k1tt3h:'       );
        $this->Add('laughing'        , ':lol:'         );
        $this->Add('leper'           , ':leper:'       );
        $this->Add('mad'             , ':mad:'         );
        $this->Add('tongue0010'      , ':p'            );
        $this->Add('popcorn'         , ':popcorn:'     );
        $this->Add('icon_redface'    , ':oops:'        );
        $this->Add('icon_cry'        , ':cry:'         );
        $this->Add('icon_twisted'    , ':evil:'        );
        $this->Add('rolleye0011'     , ':roll:'        );
        $this->Add('shocked'         , ':scream:'      );
        $this->Add('icon_wink'       , '];)'           );
        $this->Add('dead'            , ':dead:'        );
        $this->Add('pimp'            , ':pimp:'        );
        $this->Add('beerchug'        , ':beer:'        );
        $this->Add('chainsaw'        , ':chainsaw:'    );
        $this->Add('arse'            , ':moonie:'      );
        $this->Add('angel'           , ':angel:'       );
        $this->Add('bday'            , ':bday:'        );
        $this->Add('clap'            , ':clap:'        );
        $this->Add('computer'        , ':computer:'    );
        $this->Add('crash'           , ':pccrash:'     );
        $this->Add('dizzy'           , ':dizzy:'       );
        $this->Add('dodgy'           , ':naughty:'     );
        $this->Add('drink'           , ':drink:'       );
        $this->Add('facelick'        , ':lick:'        );
        $this->Add('frown'           , '>:('           );
        $this->Add('heee'            , ':hee:'         );
        $this->Add('imwithstupid'    , ':imwithstupid:');
        $this->Add('jawdrop'         , ':jawdrop:'     );
        $this->Add('king'            , ':king:'        );
        $this->Add('ladysman'        , ':ladysman:'    );
        $this->Add('mrT'             , ':mrt:'         );
        $this->Add('nurse'           , ':nurse:'       );
        $this->Add('outtahere'       , ':outtahere:'   );
        $this->Add('aaatrigger'      , ':aaatrigger:'  );
        $this->Add('repuke'          , ':repuke:'      );
        $this->Add('rofl'            , ':rofl:'        );
        $this->Add('rolling'         , ':rolling2:'    );
        $this->Add('santa'           , ':santa:'       );
        $this->Add('smash'           , ':smash:'       );
        $this->Add('toilet'          , ':toilet:'      );
        $this->Add('44'              , '~o)'           );
        $this->Add('wavey'           , ':wavey:'       );
        $this->Add('upyours'         , ':stfu:'        );
        $this->Add('fart'            , ':fart:'        );
        $this->Add('trout'           , ':trout:'       );
        $this->Add('ar15firing'      , ':machinegun:'  );
        $this->Add('microwave'       , ':microwave:'   );
        $this->Add('guillotine'      , ':guillotine:'  );
        $this->Add('poke'            , ':poke:'        );
        $this->Add('sniper'          , ':sniper:'      );
        $this->Add('monkee'          , ':monkee:'      );
        $this->Add('bandit'          , ':gringo:'      );
        $this->Add('wtf'             , ':wtf:'         );
        $this->Add('azelito'         , ':azelito:'     );
        $this->Add('crate'           , ':crate:'       );
        $this->Add('argh'            , ':-&'           );
        $this->Add('swear'           , ':swear:'       );
        $this->Add('rocketwhore'     , ':launcher:'    );
        $this->Add('skull'           , ':skull:'       );
        $this->Add('munky'           , ':munky:'       );
        $this->Add('evilgrin'        , ':E'            );
        $this->Add('banghead'        , ':brickwall:'   );
        $this->Add('wcc'             , ':wcc:'         );
        $this->Add('smiley_sherlock' , ':sherlock:'    );
        $this->Add('nag'             , ':nag:'         );
        $this->Add('rolling_eyes'    , ':rolling:'     );
        $this->Add('angryfire'       , ':flame:'       );
        $this->Add('character'       , ':ghost:'       );
        $this->Add('character0007'   , ':pirate:'      );
        $this->Add('indifferent0016' , ':zzz:'         );
        $this->Add('indifferent0002' , ':|'            );
        $this->Add('love0012'        , ':love:'        );
        $this->Add('rolleye0006'     , ':lookup:'      );
        $this->Add('sad0006'         , '];('           );
        $this->Add('scared0005'      , ':scared:'      );
        $this->Add('flail'           , ':flail:'       );
        $this->Add('emot-cowjump'    , ':cowjump:'     );
        $this->Add('emot-eng101'     , ':teach:'       );
        $this->Add('uncertain'       , ':uncertain:'   );
        $this->Add('1sm071potstir'   , ':stirring:'    );
        $this->Add('thumbs_up'       , ':thumbsup:'    );
        $this->Add('happy_open'      , ':happy:'       );
        $this->Add('snark_topic_icon', ':snark:'       );
        return $this;
    }
}