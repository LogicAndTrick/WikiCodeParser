import { Util } from '..';
import { HtmlNode } from '../Nodes/HtmlNode';
import { HtmlNodeBuilder } from '../Nodes/HtmlNodeBuilder';
import { INode } from '../Nodes/INode';
import { NodeCollection } from '../Nodes/NodeCollection';
import { ParseData } from '../ParseData';
import { Parser } from '../Parser';
import { Position } from '../Position';
import { State } from '../State';
import { Trim } from '../Util';
import { Tag } from './Tag';

export class ListTag extends Tag {
    constructor() {
        super();
        this.Token = 'list';
        this.Element = 'ul';
        this.IsBlock = true;
    }

    public override Validate(options: Record<string, string>, text: string): boolean {
        const items = text
            .split('[*]')
            .map(x => Trim(x))
            .filter(x => x?.length > 0);
        return super.Validate(options, text) && items.length > 0;
    }

    public override FormatResult(
        parser: Parser,
        data: ParseData,
        state: State,
        scope: string,
        options: Record<string, string>,
        text: string,
        tagPosition: Position,
        textPosition: Position
    ): INode {
        let before = '<' + this.Element;
        if (this.ElementClass != null) before += ' class="' + this.ElementClass + '"';
        before += '>\n';

        const content = new NodeCollection();

        const itemState = new State(textPosition.Line, textPosition.Column, text);
        let itemPos = textPosition;
        while (!itemState.Done) {
            const plainPos = itemState.SourcePosition;
            const plain = itemState.ScanTo('[*]');

            const itemNode = this.CreateListItemNode(parser, data, itemState, scope, plain, plainPos, itemPos);
            if (itemNode != null) content.Nodes.push(itemNode);

            itemPos = itemState.SourcePosition;
            itemState.Seek(3, false); // read the [*]
        }

        const after = '</' + this.Element + '>';
        const ret = new HtmlNode(tagPosition, before, content, after);
        ret.IsBlockNode = true;
        return ret;
    }

    private CreateListItemNode(parser: Parser, data: ParseData, state: State, scope: string, text: string, textPosition: Position, tagPosition: Position): HtmlNode | null {
        text = Util.Trim(text);
        if (text.length == 0) return null;

        return HtmlNodeBuilder.make('li', tagPosition)
            .add(parser.ParseTags(data, text, scope, this.TagContext(), textPosition.Line, textPosition.Column))
            .toNode({ plainBefore: '* ', plainAfter: '\n', htmlAfter: '\n' });
    }
}
