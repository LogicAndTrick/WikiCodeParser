import { Parser } from '..';
import { Colours } from '../Colours';
import { HtmlNode } from '../Nodes/HtmlNode';
import { INode } from '../Nodes/INode';
import { ParseData } from '../ParseData';
import { State } from '../State';
import { Tag } from './Tag';

export class FontTag extends Tag {
    constructor() {
        super();
        this.Token = 'font';
        this.Element = 'span';
        this.MainOption = 'color';
        this.Options = ['color', 'colour', 'size'];
    }

    public override FormatResult(parser: Parser, data: ParseData, state: State, scope: string, options: Record<string, string>, text: string): INode {
        let before = '<' + this.Element;
        if (this.ElementClass != null) before += ' class="' + this.ElementClass + '"';
        if (options['color'] || options['colour'] || options['size']) {
            before += ' style="';
            if (options['color'] && Colours.IsValidColor(options['color'])) before += 'color: ' + options['color'] + '; ';
            else if (options['colour'] && Colours.IsValidColor(options['colour'])) before += 'color: ' + options['colour'] + '; ';
            if (options['size'] && FontTag.IsValidSize(options['size'])) before += 'font-size: ' + options['size'] + 'px; ';
            before = before.trimEnd() + '"';
        }
        before += '>';
        const content = parser.ParseTags(data, text, scope, this.TagContext());
        const after = '</' + this.Element + '>';
        return new HtmlNode(before, content, after);
    }

    static IsValidSize(text: string): boolean {
        const num = parseInt(text, 10) ?? 0;
        return num >= 6 && num <= 40;
    }
}