import { HtmlNode } from '../Nodes/HtmlNode';
import { INode } from '../Nodes/INode';
import { ParseData } from '../ParseData';
import { Parser } from '../Parser';
import { State } from '../State';
import { TagParseContext } from '../TagParseContext';

export class Tag {
    public Token: string;
    public Element: string;
    public ElementClass: string | null;
    public MainOption: string;
    public Options: string[];
    public AllOptionsInMain: boolean;
    public IsBlock: boolean;
    public IsNested: boolean;

    public Scopes: string[];
    public Priority = 0;

    protected TagContext(): TagParseContext {
        return this.IsBlock ? TagParseContext.Block : TagParseContext.Inline;
    }

    constructor(token = '', element = '', elementClass: string | null = null) {
        this.Token = token;
        this.Element = element;
        this.ElementClass = elementClass;
        this.Options = [];
        this.Scopes = [];
    }

    public InScope(scope: string): boolean {
        return !scope
            || scope.trim() == ''
            || this.Scopes.includes(scope);
    }

    public Matches(state: State, token: string, context: TagParseContext): boolean {
        return token?.toLowerCase() == this.Token && (context == TagParseContext.Block || !this.IsBlock);
    }

    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    public Parse(parser: Parser, data: ParseData, state: State, scope: string, context: TagParseContext): INode {
        const index = state.Index;
        const tokenLength = this.Token.length;

        state.Seek(tokenLength + 1, false);
        let optionsString = state.ScanTo(']').trim();
        if (state.Next() != ']') {
            state.Seek(index, true);
            return null;
        }

        const options: Record<string, string> = {};
        if (optionsString.length > 0) {
            if (optionsString[0] == '=' && this.AllOptionsInMain && this.MainOption != null) {
                options[this.MainOption] = optionsString.substring(1);
            }
            else {
                if (optionsString[0] == '=') optionsString = this.MainOption + optionsString;
                const myregexp = /(?=\s|^)\s*([^ ]+?)=([^\s]*)(?=\s|$)(?!=)/img;
                let m = myregexp.exec(optionsString);
                while (m != null) {
                    const name = m[1].trim();
                    const value = m[2].trim();
                    options[name] = value;
                    m = myregexp.exec(optionsString);
                }
            }
        }

        if (this.IsNested) {
            let stack = 1;
            let text = '';
            while (!state.Done) {
                text += state.ScanTo('[');
                const tok = state.GetToken();
                if (tok.toLowerCase() == this.Token.toLowerCase()) stack++;
                if (tok.toLowerCase() == '/' + this.Token.toLowerCase() && state.Peek(tokenLength + 3).trim() == '[/' + this.Token.toLowerCase() + ']') stack--;
                if (stack == 0) {
                    state.Seek(this.Token.length + 3, false);
                    if (!this.Validate(options, text)) break;
                    return this.FormatResult(parser, data, state, scope, options, text);
                }

                text += state.Next();
            }

            state.Seek(index, true);
            console.log(2);
            return null;
        }
        else {
            const text = state.ScanTo('[/' + this.Token + ']', true);
            if (state.Peek(tokenLength + 3).trim() == '[/' + this.Token.toLowerCase() + ']' && this.Validate(options, text)) {
                state.Seek(this.Token.length + 3, false);
                return this.FormatResult(parser, data, state, scope, options, text);
            }
            else {
                state.Seek(index, true);
                console.log(3);
                return null;
            }
        }
    }

    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    public Validate(options: Record<string, string>, text: string): boolean {
        return true;
    }

    public FormatResult(parser: Parser, data: ParseData, state: State, scope: string, options: Record<string, string>, text: string): INode {
        let before = '<' + this.Element;
        if (this.ElementClass != null) before += ' class="' + this.ElementClass + '"';
        before += '>';
        const after = '</' + this.Element + '>';
        const content = parser.ParseTags(data, text, scope, this.TagContext());
        const ret = new HtmlNode(before, content, after);
        ret.IsBlockNode = this.IsBlock;
        return ret;
    }


    // Extensions
    
    public WithScopes(...scopes : string[])
    {
        this.Scopes = scopes;
        return this;
    }

    public WithToken(token : string)
    {
        this.Token = token;
        return this;
    }

    public WithElement(element : string)
    {
        this.Element = element;
        return this;
    }

    public WithElementClass(elementClass : string)
    {
        this.ElementClass = elementClass;
        return this;
    }

    public WithBlock(isBlock : boolean)
    {
        this.IsBlock = isBlock;
        return this;
    }
}
