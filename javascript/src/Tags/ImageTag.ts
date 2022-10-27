import { Parser } from '..';
import { HtmlHelper } from '../HtmlHelper';
import { HtmlNode } from '../Nodes/HtmlNode';
import { INode } from '../Nodes/INode';
import { PlainTextNode } from '../Nodes/PlainTextNode';
import { ParseData } from '../ParseData';
import { State } from '../State';
import { Tag } from './Tag';

export class ImageTag extends Tag {
    constructor() {
        super();
        this.Token = 'img';
        this.Element = 'div';
        this.MainOption = 'url';
        this.Options = ['url'];
        this.IsBlock = true;
    }

    public FormatResult(_parser: Parser, _data: ParseData, state: State, _scope: string, options: Record<string, string>, text: string): INode {
        let url = text;
        if (options['url']) url = options['url'];
        if (!url.match(/^([a-z]{2,10}:\/\/)/i)) url = 'http://' + url;
        url = HtmlHelper.AttributeEncode(url);

        const classes = ['embedded', 'image'];
        if (this.ElementClass != null) classes.push(this.ElementClass);
        let element = this.Element;

        if (!this.IsBlock) {
            element = 'span';
            classes.push('inline');
        } else {
            state.SkipWhitespace();
        }

        const before = `<${element} class="${classes.join(' ')}">` +
            '<span class="caption-panel">' +
            `<img class="caption-body" src="${url}" alt="User posted image" />`;
        const after = `</span></${element}>`;
        const plainsp = element == 'div' ? '\n' : '';
        const ret = new HtmlNode(before, PlainTextNode.Empty(), after);
        ret.PlainBefore = `${plainsp}[User posted image]${plainsp}`;
        ret.IsBlockNode = element == 'div';
        return ret;
    }

    public Validate(options: Record<string, string>, text: string): boolean {
        let url = text;
        if (options['url']) url = options['url'];
        return !url.includes('<script') && url.match(/^([a-z]{2,10}:\/\/)?([^\]"\n ]+?)$/i) != null;
    }
}