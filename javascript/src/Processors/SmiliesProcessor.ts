import { Parser } from '..';
import { HtmlHelper } from '../HtmlHelper';
import { HtmlNode } from '../Nodes/HtmlNode';
import { INode } from '../Nodes/INode';
import { PlainTextNode } from '../Nodes/PlainTextNode';
import { UnprocessablePlainTextNode } from '../Nodes/UnprocessablePlainTextNode';
import { ParseData } from '../ParseData';
import { IndexOfAny, Template } from '../Util';
import { INodeProcessor } from './INodeProcessor';

class SmileyDefinition {
    public Name : string;
    public Tokens : string[];

    constructor(name : string, tokens : string[])
    {
        this.Name = name;
        this.Tokens = tokens;
    }

    public GetMatchingToken(text : string, startIndex : number) : string {
        for (const token of this.Tokens) {
            if (text.indexOf(token, startIndex) == startIndex) {
                // Must end with whitespace
                if (startIndex + token.length < text.length - 1 && text[startIndex + token.length].trim() != '') continue;
                return token;
            }
        }
        return null;
    }
}

export class SmiliesProcessor implements INodeProcessor {
    public Priority = 5;
    public UrlFormatString : string;

    private _definitions : SmileyDefinition[];
    private _initialised : boolean;
    private _tokenStarts : Set<string>;

    private static MaxSmilies = 100;

    constructor(urlFormatString : string) {
        this.UrlFormatString = urlFormatString;
        this._definitions = [];
        this._initialised = false;
    }

    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    ShouldProcess(node: INode, _scope: string): boolean {
        return node instanceof PlainTextNode && this._definitions.length > 0;
    }

    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    Process(_parser: Parser, _data: ParseData, node: INode, _scope: string): INode[] {
        if (!this._initialised) {
            this._tokenStarts = new Set<string>(this._definitions.flatMap(x => x.Tokens).map(x => x[0]));
            this._initialised = true;
        }

        const ret: INode[] = [];

        const text = (node as PlainTextNode).Text;
        let start = 0;
        let index = -1;
        let numSmilies = 0;
        while (index + 1 < text.length && (index = IndexOfAny(text, this._tokenStarts, index + 1)) >= 0) {
            if (numSmilies > SmiliesProcessor.MaxSmilies) {
                ret.push(new HtmlNode('<em class="text-danger">', new UnprocessablePlainTextNode(' [warning: too many smilies in post] '), '</em>'));
                break;
            }

            // Must start with whitespace
            if (index != 0 && text[index - 1].trim() != '') continue;

            // Find an appropriate definition
            let definition: SmileyDefinition | null = null;
            let token: string | null = null;
            for (const def of this._definitions) {
                token = def.GetMatchingToken(text, index);
                if (token == null) continue;

                definition = def;
                break;
            }
            if (definition == null) continue;

            // Must end with whitespace
            if (index + token.length < text.length - 1 && text[index + token.length].trim() != '') continue;

            // We have a smiley
            if (start < index) ret.push(new PlainTextNode(text.substring(start, index)));
            const src = HtmlHelper.AttributeEncode(Template(this.UrlFormatString, { 0: definition.Name }));
            const alt = HtmlHelper.AttributeEncode(token);
            const node = new HtmlNode(`<img class="smiley" src="${src}" alt="${alt}" />`, PlainTextNode.Empty(), '');
            node.PlainBefore = token;
            ret.push(node);
            start = index + token.length;
            index += token.length;

            numSmilies++;
        }

        if (start < text.length) ret.push(new PlainTextNode(text.substring(start)));
        return ret;
    }

    public Add(name: string, ...tokens: string[]): SmiliesProcessor {
        this._definitions.push(new SmileyDefinition(name, tokens));
        this._initialised = false;
        return this;
    }

    public AddTwhl() : SmiliesProcessor {
        this.Add('aggrieved'   , ':aggrieved:'              );
        this.Add('aghast'      , ':aghast:'                 );
        this.Add('angry'       , ':x', ':-x', ':angry:'     );
        this.Add('badass'      , ':badass:'                 );
        this.Add('confused'    , ':confused:'               );
        this.Add('cry'         , ':cry:'                    );
        this.Add('cyclops'     , ':cyclops:'                );
        this.Add('lol'         , ':lol:'                    );
        this.Add('frown'       , ':|', ':-|', ':frown:'     );
        this.Add('furious'     , ':furious:'                );
        this.Add('glad'        , ':glad:'                   );
        this.Add('heart'       , ':heart:'                  );
        this.Add('grin'        , ':D', ':-D', ':grin:'      );
        this.Add('nervous'     , ':nervous:'                );
        this.Add('nuke'        , ':nuke:'                   );
        this.Add('nuts'        , ':nuts:'                   );
        this.Add('quizzical'   , ':quizzical:'              );
        this.Add('rollseyes'   , ':roll:', ':rollseyes:'    );
        this.Add('sad'         , ':(', ':-(', ':sad:'       );
        this.Add('smile'       , ':)', ':-)', ':smile:'     );
        this.Add('surprised'   , ':o', ':-o', ':surprised:' );
        this.Add('thebox'      , ':thebox:'                 );
        this.Add('thefinger'   , ':thefinger:'              );
        this.Add('tired'       , ':tired:'                  );
        this.Add('tongue'      , ':P', ':-P', ':tongue:'    );
        this.Add('toocool'     , ':cool:'                   );
        this.Add('unsure'      , ':\\', ':-\\', ':unsure:'  );
        this.Add('biggrin'     , ':biggrin:'                );
        this.Add('wink'        , ';)', ';-)', ':wink:'      );
        this.Add('zonked'      , ':zonked:'                 );
        this.Add('sarcastic'   , ':sarcastic:'              );
        this.Add('combine'     , ':combine:', ':elite:'     );
        this.Add('gak'         , ':gak:'                    );
        this.Add('animehappy'  , ':^_^:'                    );
        this.Add('pwnt'        , ':pwned:'                  );
        this.Add('target'      , ':target:'                 );
        this.Add('ninja'       , ':ninja:'                  );
        this.Add('hammer'      , ':hammer:'                 );
        this.Add('pirate'      , ':pirate:', ':yar:'        );
        this.Add('walter'      , ':walter:'                 );
        this.Add('plastered'   , ':plastered:'              );
        this.Add('bigmouth'    , ':zomg:'                   );
        this.Add('brokenheart' , ':heartbreak:'             );
        this.Add('ciggiesmilie', ':ciggie:'                 );
        this.Add('combines'    , ':combines:'               );
        this.Add('crowbar'     , ':crowbar:'                );
        this.Add('death'       , ':death:'                  );
        this.Add('freeman'     , ':freeman:'                );
        this.Add('hecu'        , ':hecu:'                   );
        this.Add('nya'         , ':nya:'                    );
        return this;
    }
    
    public AddSnarkpit() : SmiliesProcessor {
        this.Add('icon_biggrin'    , ':D');
        this.Add('sailor'          , ':sailor:'      );
        this.Add('icon_smile'      , ':)', ':-)'     );
        this.Add('dorky'           , ':geek:'        );
        this.Add('sad0019'         , ':('            );
        this.Add('icon_eek'        , ':-o'           );
        this.Add('grenade'         , ':grenade:'     );
        this.Add('confused'        , ':confused:'    );
        this.Add('icon_cool'       , '8-)'           );
        this.Add('kitty'           , ':k1tt3h:'      );
        this.Add('laughing'        , ':lol:'         );
        this.Add('leper'           , ':leper:'       );
        this.Add('mad'             , ':mad:'         );
        this.Add('tongue0010'      , ':p'            );
        this.Add('popcorn'         , ':popcorn:'     );
        this.Add('icon_redface'    , ':oops:'        );
        this.Add('icon_cry'        , ':cry:'         );
        this.Add('icon_twisted'    , ':evil:'        );
        this.Add('rolleye0011'     , ':roll:'        );
        this.Add('shocked'         , ':scream:'      );
        this.Add('icon_wink'       , ';)'            );
        this.Add('dead'            , ':dead:'        );
        this.Add('pimp'            , ':pimp:'        );
        this.Add('beerchug'        , ':beer:'        );
        this.Add('chainsaw'        , ':chainsaw:'    );
        this.Add('arse'            , ':moonie:'      );
        this.Add('angel'           , ':angel:'       );
        this.Add('bday'            , ':bday:'        );
        this.Add('clap'            , ':clap:'        );
        this.Add('computer'        , ':computer:'    );
        this.Add('crash'           , ':pccrash:'     );
        this.Add('dizzy'           , ':dizzy:'       );
        this.Add('dodgy'           , ':naughty:'     );
        this.Add('drink'           , ':drink:'       );
        this.Add('facelick'        , ':lick:'        );
        this.Add('frown'           , '>:('           );
        this.Add('heee'            , ':hee:'         );
        this.Add('imwithstupid'    , ':imwithstupid:');
        this.Add('jawdrop'         , ':jawdrop:'     );
        this.Add('king'            , ':king:'        );
        this.Add('ladysman'        , ':ladysman:'    );
        this.Add('mrT'             , ':mrt:'         );
        this.Add('nurse'           , ':nurse:'       );
        this.Add('outtahere'       , ':outtahere:'   );
        this.Add('aaatrigger'      , ':aaatrigger:'  );
        this.Add('repuke'          , ':repuke:'      );
        this.Add('rofl'            , ':rofl:'        );
        this.Add('rolling'         , ':rolling2:'    );
        this.Add('santa'           , ':santa:'       );
        this.Add('smash'           , ':smash:'       );
        this.Add('toilet'          , ':toilet:'      );
        this.Add('44'              , '~o)'           );
        this.Add('wavey'           , ':wavey:'       );
        this.Add('upyours'         , ':stfu:'        );
        this.Add('fart'            , ':fart:'        );
        this.Add('trout'           , ':trout:'       );
        this.Add('ar15firing'      , ':machinegun:'  );
        this.Add('microwave'       , ':microwave:'   );
        this.Add('guillotine'      , ':guillotine:'  );
        this.Add('poke'            , ':poke:'        );
        this.Add('sniper'          , ':sniper:'      );
        this.Add('monkee'          , ':monkee:'      );
        this.Add('bandit'          , ':gringo:'      );
        this.Add('wtf'             , ':wtf:'         );
        this.Add('azelito'         , ':azelito:'     );
        this.Add('crate'           , ':crate:'       );
        this.Add('argh'            , ':-&'           );
        this.Add('swear'           , ':swear:'       );
        this.Add('rocketwhore'     , ':launcher:'    );
        this.Add('skull'           , ':skull:'       );
        this.Add('munky'           , ':munky:'       );
        this.Add('evilgrin'        , ':E'            );
        this.Add('banghead'        , ':brickwall:'   );
        this.Add('wcc'             , ':wcc:'         );
        this.Add('smiley_sherlock' , ':sherlock:'    );
        this.Add('nag'             , ':nag:'         );
        this.Add('rolling_eyes'    , ':rolling:'     );
        this.Add('angryfire'       , ':flame:'       );
        this.Add('character'       , ':ghost:'       );
        this.Add('character0007'   , ':pirate:'      );
        this.Add('indifferent0016' , ':zzz:'         );
        this.Add('indifferent0002' , ':|'            );
        this.Add('love0012'        , ':love:'        );
        this.Add('rolleye0006'     , ':lookup:'      );
        this.Add('sad0006'         , ';('            );
        this.Add('scared0005'      , ':scared:'      );
        this.Add('flail'           , ':flail:'       );
        this.Add('emot-cowjump'    , ':cowjump:'     );
        this.Add('emot-eng101'     , ':teach:'       );
        this.Add('uncertain'       , ':uncertain:'   );
        this.Add('1sm071potstir'   , ':stirring:'    );
        this.Add('thumbs_up'       , ':thumbsup:'    );
        this.Add('happy_open'      , ':happy:'       );
        this.Add('snark_topic_icon', ':snark:'       );
        return this;
    }
}
