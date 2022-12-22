import { Parser } from '..';
import { HtmlNode } from '../Nodes/HtmlNode';
import { INode } from '../Nodes/INode';
import { ParseData } from '../ParseData';
import { State } from '../State';
import { FontTag } from './FontTag';
import { Tag } from './Tag';

export class SizeTag extends Tag {
    constructor() {
        super();
        this.Token = 'size';
        this.Element = 'span';
        this.MainOption = 'size';
        this.Options = ['size'];
        this.AllOptionsInMain = true;
    }

    public override FormatResult(parser: Parser, data: ParseData, state: State, scope: string, options: Record<string, string>, text: string): INode {
        let before = '<' + this.Element;
        if (this.ElementClass != null) before += ' class="' + this.ElementClass + '"';
        if (options['size']) {
            before += ' style="';
            if (options['size'] && FontTag.IsValidSize(options['size'])) before += 'font-size: ' + options['size'] + 'px; ';
            before = before.trimEnd() + '"';
        }
        before += '>';
        const content = parser.ParseTags(data, text, scope, this.TagContext());
        const after = '</' + this.Element + '>';
        return new HtmlNode(before, content, after);
    }
}