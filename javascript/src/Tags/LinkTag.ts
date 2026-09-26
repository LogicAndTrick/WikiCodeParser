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
        const url = HtmlHelper.AttributeEncode(this.BuildUrl(options, text));

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
        const url = this.BuildUrl(options, text);
        return HtmlHelper.ValidateUrl(url) && url.match(/^[^\]"\n ]+$/i) != null;
    }

    private BuildUrl(options: Record<string, string>, text: string): string {
        let url = text;
        if (options['url']) url = options['url'];
        url = HtmlHelper.StripControlCharacters(url);
        if (this.Token == 'email') url = 'mailto:' + url;
        else if (HtmlHelper.GetUrlScheme(url) == null) url = 'http://' + url;
        return url;
    }
}