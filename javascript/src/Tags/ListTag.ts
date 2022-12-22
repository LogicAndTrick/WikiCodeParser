import { Parser } from '..';
import { HtmlNode } from '../Nodes/HtmlNode';
import { INode } from '../Nodes/INode';
import { NodeCollection } from '../Nodes/NodeCollection';
import { ParseData } from '../ParseData';
import { State } from '../State';
import { Tag } from './Tag';

export class ListTag extends Tag {
    constructor() {
        super();
        this.Token = 'list';
        this.Element = 'ul';
        this.IsBlock = true;
    }

    public override Validate(options: Record<string, string>, text: string): boolean {
        const items = text.split('[*]')
            .map(x => x.trim())
            .filter(x => x?.length > 0);
        return super.Validate(options, text) && items.length > 0;
    }

    public override FormatResult(parser: Parser, data: ParseData, state: State, scope: string, options: Record<string, string>, text: string): INode {
        let before = '<' + this.Element;
        if (this.ElementClass != null) before += ' class="' + this.ElementClass + '"';
        before += '>\n';

        const content = new NodeCollection();
        const items = text.split('[*]')
            .map(x => x.trim())
            .filter(x => x?.length > 0);
        for (const item of items) {
            const node = new HtmlNode('<li>', parser.ParseTags(data, item, scope, this.TagContext()), '</li>\n');
            node.PlainBefore = '* ';
            node.PlainAfter = '\n';
            content.Nodes.push(node);
        }
        
        const after = '</' + this.Element + '>';
        const ret = new HtmlNode(before, content, after);
        ret.IsBlockNode = true;
        return ret;
    }
}
