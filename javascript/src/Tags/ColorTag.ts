import { Parser } from '..';
import { Colours } from '../Colours';
import { HtmlNode } from '../Nodes/HtmlNode';
import { INode } from '../Nodes/INode';
import { ParseData } from '../ParseData';
import { State } from '../State';
import { Tag } from './Tag';

export class ColorTag extends Tag {
    constructor() {
        super();
        this.Token = 'color';
        this.Element = 'span';
        this.MainOption = 'color';
        this.Options = ['color'];
        this.AllOptionsInMain = true;
    }

    public override FormatResult(parser: Parser, data: ParseData, state: State, scope: string, options: Record<string, string>, text: string): INode {
        let before = '<' + this.Element;
        if (this.ElementClass != null) before += ' class="' + this.ElementClass + '"';
        if (options['color'] || options['colour']) {
            before += ' style="';
            if (options['color'] && Colours.IsValidColor(options['color'])) before += 'color: ' + options['color'] + '; ';
            else if (options['colour'] && Colours.IsValidColor(options['colour'])) before += 'color: ' + options['colour'] + '; ';
            before = before.trimEnd() + '"';
        }
        before += '>';
        const content = parser.ParseTags(data, text, scope, this.TagContext());
        const after = '</' + this.Element + '>';
        return new HtmlNode(before, content, after);
    }
}