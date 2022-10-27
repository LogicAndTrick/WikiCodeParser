import { Parser } from '..';
import { PreElement } from '../Elements/PreElement';
import { HtmlNode } from '../Nodes/HtmlNode';
import { INode } from '../Nodes/INode';
import { UnprocessablePlainTextNode } from '../Nodes/UnprocessablePlainTextNode';
import { ParseData } from '../ParseData';
import { State } from '../State';
import { Tag } from './Tag';

export class PreTag extends Tag {
    constructor() {
        super();
        this.Token = 'pre';
        this.Element = 'pre';
        this.IsBlock = true;
    }

    public override FormatResult(_parser: Parser, _data: ParseData, _state: State, _scope: string, _options: Record<string, string>, text: string): INode {
        let before = '<' + this.Element;
        if (this.ElementClass != null) before += ' class="' + this.ElementClass + '"';
        before += '><code>';
        const after = '</code></' + this.Element + '>';

        let arr = text.split('\n');

        // Trim blank lines from the start and end of the array
        for (let i = 0; i < 2; i++) {
            while (arr.length > 0 && arr[0].trim() == '') arr.splice(0, 1);
            arr.reverse();
        }

        arr = PreElement.FixCodeIndentation(arr);
        text = arr.join('\n');

        const ret = new HtmlNode(before, new UnprocessablePlainTextNode(text), after);
        ret.IsBlockNode = true;
        return ret;
    }
}