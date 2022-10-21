import { Parser } from '..';
import { HtmlNode } from '../Nodes/HtmlNode';
import { INode } from '../Nodes/INode';
import { UnprocessablePlainTextNode } from '../Nodes/UnprocessablePlainTextNode';
import { ParseData } from '../ParseData';
import { State } from '../State';
import { Tag } from './Tag';

export class CodeTag extends Tag {
    constructor() {
        super();
        this.Token = 'code';
        this.Element = 'code';
    }

    public override FormatResult(_parser: Parser, _data: ParseData, _state: State, _scope: string, _options: Record<string, string>, text: string): INode {
        return new HtmlNode('<code>', new UnprocessablePlainTextNode(text), '</code>');
    }
}
