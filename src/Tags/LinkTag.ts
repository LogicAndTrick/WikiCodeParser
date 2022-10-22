import { Parser } from '..';
import { HtmlHelper } from '../HtmlHelper';
import { HtmlNode } from '../Nodes/HtmlNode';
import { INode } from '../Nodes/INode';
import { UnprocessablePlainTextNode } from '../Nodes/UnprocessablePlainTextNode';
import { ParseData } from '../ParseData';
import { State } from '../State';
import { Tag } from './Tag';

export class LinkTag extends Tag {
    constructor() {
        super();
        this.Token = 'url';
        this.Element = 'a';
        this.MainOption = 'url';
        this.Options = ['url'];
    }

    public FormatResult(parser: Parser, data: ParseData, state: State, scope: string, options: Record<string, string>, text: string): INode {
        let url = text;
        if (options['url']) url = options['url'];
        if (this.Token == 'email') url = 'mailto:' + url;
        else if (!url.match(/^([a-z]{2,10}:\/\/)/i)) url = 'http://' + url;
        url = HtmlHelper.AttributeEncode(url);

        const classes = [];
        if (this.ElementClass != null) classes.push(this.ElementClass);

        const before = `<${this.Element} ` + (classes.length > 0 ? `class="${classes.join(' ')}" ` : '') + `href="${url}">`;
        const after = `</${this.Element}>`;

        const content = options['url']
            ? parser.ParseTags(data, text, scope, this.TagContext())
            : new UnprocessablePlainTextNode(text);
        return new HtmlNode(before, content, after);
    }

    public Validate(options: Record<string, string>, text: string): boolean {
        let url = text;
        if (options['url']) url = options['url'];
        return !url.includes('<script') && url.match(/^([a-z]{2,10}:\/\/)?([^\]"\n ]+?)$/i) != null;
    }
}