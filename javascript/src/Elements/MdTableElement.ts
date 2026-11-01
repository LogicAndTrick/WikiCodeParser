import { Lines } from '../Lines';
import { HtmlNode } from '../Nodes/HtmlNode';
import { INode } from '../Nodes/INode';
import { NodeCollection } from '../Nodes/NodeCollection';
import { RefNode } from '../Nodes/RefNode';
import { ParseData } from '../ParseData';
import { Parser } from '../Parser';
import { TagParseContext } from '../TagParseContext';
import { Trim, TrimEnd } from '../Util';
import { Element } from './Element';

class TableCell implements INode {
    public Type: string;
    public Contents: INode;

    constructor(type: string, contents: INode) {
        this.Type = type;
        this.Contents = contents;
    }

    ToHtml() {
        return `<${this.Type}>${this.Contents.ToHtml()}</${this.Type}>`;
    }

    ToPlainText() {
        return this.Contents.ToPlainText();
    }

    GetChildren(): INode[] {
        return [this.Contents];
    }

    ReplaceChild(i: number, node: INode) {
        if (i != 0) throw new Error('Argument out of range');
        this.Contents = node;
    }

    HasContent(): boolean {
        return this.Contents.HasContent();
    }
}

class TableRow implements INode {
    public Cells: TableCell[];

    constructor(...cells: TableCell[]) {
        this.Cells = cells;
    }

    ToHtml(): string {
        let sb = '<tr>\n';
        for (const cell of this.Cells) {
            sb += cell.ToHtml() + '\n';
        }
        sb += '</tr>\n';
        return sb;
    }

    ToPlainText(): string {
        let sb = '';
        let first = true;
        for (const cell of this.Cells) {
            if (!first) sb += ' | ';
            sb += cell.ToPlainText();
            first = false;
        }
        sb += '\n';
        return sb;
    }

    GetChildren(): INode[] {
        return [...this.Cells];
    }

    ReplaceChild(i: number, node: INode): void {
        this.Cells[i] = node as TableCell;
    }

    HasContent(): boolean {
        return true;
    }
}

export class MdTableElement extends Element {
    public Matches(lines: Lines): boolean {
        const value = TrimEnd(lines.Value());
        return value.length >= 2 && value[0] == '|' && (value[1] == '=' || value[1] == '-');
    }
    public Consume(parser: Parser, data: ParseData, lines: Lines, scope: string): INode | null {
        const arr: TableRow[] = [];
        do {
            const value = TrimEnd(lines.Value());
            if (value.length < 2 || value[0] != '|' || (value[1] != '=' && value[1] != '-')) {
                lines.Back();
                break;
            }
            const type = value[1] == '=' ? 'th' : 'td';
            const cells = MdTableElement.SplitTable(value.substring(2)).map(x => new TableCell(type, MdTableElement.ResolveCell(x, parser, data, scope)));
            arr.push(new TableRow(...cells));
        } while (lines.Next());

        return new HtmlNode('<div class="table-responsive"><table class="table table-bordered">', new NodeCollection(...arr), '</table></div>');
    }

    private static SplitTable(text: string): string[] {
        const ret = [];
        let level = 0;
        let last = 0;
        text = Trim(text);
        const len = text.length;
        let i: number;
        for (i = 0; i < len; i++) {
            const c = text[i];
            if (c == '[') level++;
            else if (c == ']') {
                if (level > 0) level--;
            } else if ((c == '|' && level == 0) || i == len - 1) {
                ret.push(Trim(text.substring(last, i + (i == len - 1 ? 1 : 0))));
                last = i + 1;
            }
        }
        if (last < len) ret.push(Trim(text.substring(last, i + (i == len - 1 ? 1 : 0))));
        return ret;
    }

    private static ResolveCell(text: string, parser: Parser, data: ParseData, scope: string): INode {
        const res = /^:ref=([a-z0-9 ]+)$/i.exec(Trim(text));
        if (res) {
            const name = res[1];
            return new RefNode(data, name);
        }
        return parser.ParseTags(data, text, scope, TagParseContext.Block);
    }
}
