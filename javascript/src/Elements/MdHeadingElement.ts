import { HtmlHelper } from '../HtmlHelper';
import { Lines } from '../Lines';
import { INode } from '../Nodes/INode';
import { ParseData } from '../ParseData';
import { Parser } from '../Parser';
import { TagParseContext } from '../TagParseContext';
import { Trim } from '../Util';
import { Element } from './Element';

class HeadingNode implements INode {
    public Level: number;
    public ID: string;
    public Text: INode;
    constructor(level: number, id: string, text: INode) {
        this.Level = level;
        this.ID = id;
        this.Text = text;
    }

    ToHtml(): string {
        const escaped = HtmlHelper.AttributeEncode(this.ID);
        return `<h${this.Level} id="${escaped}">${this.Text.ToHtml()}</h${this.Level}>`;
    }

    ToPlainText(): string {
        const plain = this.Text.ToPlainText().replace(/\n/g, ' ');
        return plain + '\n' + '-'.repeat(plain.length);
    }

    GetChildren(): INode[] {
        return [this.Text];
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

    public Consume(parser: Parser, data: ParseData, lines: Lines, scope: string): INode | null {
        const value = Trim(lines.Value());
        const res = /^(=+)(.*?)=*$/i.exec(value)!;
        const level = Math.min(6, res[1].length);
        const text = Trim(res[2]);

        const contents = parser.ParseTags(data, text, scope, TagParseContext.Inline);
        const contentsPlainText = parser.RunProcessors(contents, data, scope).ToPlainText();
        const id = MdHeadingElement.GetUniqueAnchor(data, contentsPlainText);
        return new HeadingNode(level, id, contents);
    }

    private static GetUniqueAnchor(data: ParseData, text: string): string {
        const key = MdHeadingElement.name + '.IdList';
        const anchors = data.Get(key, () => new Set<string>());

        const id = text.replace(/[^0-9A-Za-z?/:@\-._~!$&'()*+,;=]+/gu, '_');
        let anchor = id;
        let inc = 1;
        do {
            // Increment if we have a duplicate
            if (!anchors.has(anchor)) break;
            inc++;
            anchor = `${id}_${inc}`;
        } while (true);

        anchors.add(anchor);
        return anchor;
    }
}
