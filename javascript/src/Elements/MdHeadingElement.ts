import { Parser } from '..';
import { Lines } from '../Lines';
import { INode } from '../Nodes/INode';
import { ParseData } from '../ParseData';
import { TagParseContext } from '../TagParseContext';
import { Element } from './Element';

class HeadingNode implements INode {
    public Level : number;
    public ID : string;
    public Text : INode;
    constructor(level : number, id : string, text : INode) {
        this.Level = level;
        this.ID = id;
        this.Text = text;
    }

    ToHtml(): string {
        return `<h${this.Level} id="${this.ID}">${this.Text.ToHtml()}</h${this.Level}>`;
    }

    ToPlainText(): string {
        const plain = this.Text.ToPlainText().replace(/\n/g, ' ');
        return plain + '\n' + '-'.repeat(plain.length);
    }

    GetChildren(): INode[] {
        return [ this.Text ];
    }

    ReplaceChild(i: number, node: INode): void {
        if (i != 0) throw new Error('Argument out of range');
        this.Text = node;
    }

    HasContent(): boolean {
        return true;
    }
}

export class MdHeadingElement extends Element {
    public Matches(lines: Lines): boolean {
        const value = lines.Value();
        return value.length > 0 && value.startsWith('=');
    }
    public Consume(parser: Parser, data: ParseData, lines: Lines, scope: string): INode {
        const value = lines.Value().trim();
        const res = /^(=+)(.*?)=*$/i.exec(value);
        const level = Math.min(6, res[1].length);
        const text = res[2].trim();

        let contents = parser.ParseTags(data, text, scope, TagParseContext.Inline);
        contents = parser.RunProcessors(contents, data, scope);
        const id = MdHeadingElement.GetUniqueAnchor(data, contents.ToPlainText());
        return new HeadingNode(level, id, contents);
        
    }

    private static GetUniqueAnchor(data : ParseData, text : string) : string
    {
        const key = MdHeadingElement.name + '.IdList';
        const anchors = data.Get(key, () => new Set<string>());

        const id = text.replace(/[^\da-z?/:@\-._~!$&'()*+,;=]/ig, '_');
        let anchor = id;
        let inc = 1;
        do {
            // Increment if we have a duplicate
            if (!anchors.has(anchor)) break;
            inc++;
            anchor = `${id}_${inc}`;
        // eslint-disable-next-line no-constant-condition
        } while (true);

        anchors.add(anchor);
        return anchor;
    }
}