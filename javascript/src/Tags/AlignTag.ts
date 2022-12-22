import { Parser } from '..';
import { HtmlNode } from '../Nodes/HtmlNode';
import { INode } from '../Nodes/INode';
import { ParseData } from '../ParseData';
import { State } from '../State';
import { Tag } from './Tag';

export class AlignTag extends Tag {
    constructor() {
        super();
        this.Token = 'align';
        this.Element = 'div';
        this.MainOption = 'align';
        this.Options = ['align'];
        this.AllOptionsInMain = true;
        this.IsBlock = true;
    }

    public override FormatResult(parser: Parser, data: ParseData, state: State, scope: string, options: Record<string, string>, text: string): INode {
        let before = '<' + this.Element;
        let cls = (this.ElementClass || '') + ' ';
        if (options['align'] && AlignTag.IsValidAlign(options['align'])) {
            cls += 'text-' + AlignTag.ConvertAlign(options['align']);
        }
        before += ' class="' + cls.trim() + '">';
        const content = parser.ParseTags(data, text, scope, this.TagContext());
        const after = '</' + this.Element + '>';
        const ret = new HtmlNode(before, content, after);
        ret.IsBlockNode = true;
        return ret;
    }

    private static IsValidAlign(text : string) : boolean {
        return text == 'left' || text == 'right' || text == 'center';
    }

    private static ConvertAlign(text: string) : string {
        if (text == 'left') return 'start';
        if (text == 'right') return 'end';
        return 'center';
    }
}
