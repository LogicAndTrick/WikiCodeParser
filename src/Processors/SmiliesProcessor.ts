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
            if (text.indexOf(token, startIndex) == startIndex) return token;
        }
        return null;
    }
}

export class SmiliesProcessor implements INodeProcessor {
    public Priority = 5;
    public UrlFormatString = 'https://twhl.info/images/smilies/{0}.png';

    private _definitions : SmileyDefinition[];
    private _initialised : boolean;
    private _tokenStarts : Set<string>;

    private static MaxSmilies = 100;

    constructor() {
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

    public AddDefault() : SmiliesProcessor {
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
}
