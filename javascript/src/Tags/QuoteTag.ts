import { Parser } from '..';
import { HtmlNode } from '../Nodes/HtmlNode';
import { INode } from '../Nodes/INode';
import { ParseData } from '../ParseData';
import { State } from '../State';
import { Tag } from './Tag';

export class QuoteTag extends Tag {
    constructor() {
        super();
        this.Token = 'quote';
        this.Element = 'blockquote';
        this.MainOption = 'name';
        this.Options = ['name'];
        this.AllOptionsInMain = true;
        this.IsBlock = true;
        this.IsNested = true;
    }

    public override FormatResult(parser: Parser, data: ParseData, state: State, scope: string, options: Record<string, string>, text: string): INode {
        let before = '<' + this.Element;
        if (this.ElementClass != null) before += ' class="' + this.ElementClass + '"';
        before += '>';
        if (options['name']) {
            before += '<strong class="quote-name">' + options['name'] + ' said:</strong><br/>';
        }
        const after = '</' + this.Element + '>';
        const content = parser.ParseTags(data, text?.trim(), scope, this.TagContext());
        const ret = new HtmlNode(before, content, after);
        ret.PlainBefore = (options['name'] ? options['name'] + ' said: ' : '') + '[quote]\n';
        ret.PlainAfter = '\n[/quote]';
        ret.IsBlockNode = this.IsBlock;
        return ret;
    }
}