import { Lines } from '../Lines';
import { HtmlNode } from '../Nodes/HtmlNode';
import { INode } from '../Nodes/INode';
import { NodeCollection } from '../Nodes/NodeCollection';
import { RefNode } from '../Nodes/RefNode';
import { ParseData } from '../ParseData';
import { Parser } from '../Parser';
import { Position } from '../Position';
import { TagParseContext } from '../TagParseContext';
import { TrimEnd, TrimStart } from '../Util';
import { Element } from './Element';

class TableCell implements INode {
    public Line: number = 0;
    public Column: number = 0;
    public Type: string;
    public Contents: INode;

    constructor(line: number, column: number, type: string, contents: INode) {
        this.Line = line;
        this.Column = column;
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
    public Line: number = 0;
    public Column: number = 0;
    public Cells: TableCell[];

    constructor(line: number, column: number, ...cells: TableCell[]) {
        this.Line = line;
        this.Column = column;
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
        const startLine = lines.SourceLine;
        const arr: TableRow[] = [];
        do {
            const value = lines.Value();
            if (value.length < 2 || value[0] != '|' || (value[1] != '=' && value[1] != '-')) {
                lines.Back();
                break;
            }
            const line = lines.SourceLine;
            const type = value[1] == '=' ? 'th' : 'td';
            const cells = MdTableElement.SplitTable(2, value).map(x => new TableCell(line, x.col, type, MdTableElement.ResolveCell(x.text, parser, data, scope, line, x.col)));
            arr.push(new TableRow(line, 0, ...cells));
        } while (lines.Next());

        return new HtmlNode(new Position(startLine, 0), '<div class="table-responsive"><table class="table table-bordered">', new NodeCollection(...arr), '</table></div>');
    }

    private static SplitTable(start: number, text: string): { col: number; text: string }[] {
        const ret = [];
        let level = 0;
        let last = start;
        const len = text.length;
        let i: number;
        for (i = start; i < len; i++) {
            const c = text[i];
            if (c == '[') level++;
            else if (c == ']') {
                if (level > 0) level--;
            } else if ((c == '|' && level == 0) || i == len - 1) {
                ret.push(this.CreateTableText(text, last, i));
                last = i + 1;
            }
        }
        if (last < len) ret.push(this.CreateTableText(text, last, i));
        return ret;
    }

    private static CreateTableText(text: string, start: number, end: number): { col: number; text: string } {
        // get the text length
        let col = start;
        const txt = text.substring(start, end + (end == text.length - 1 ? 1 : 0));

        // trim the start of the text - the number of removed chars is how much we need to add to the column
        const ol = txt.length;
        const ts = TrimStart(txt);
        col += ol - ts.length;

        return { col, text: TrimEnd(ts) };
    }

    private static ResolveCell(text: string, parser: Parser, data: ParseData, scope: string, line: number, column: number): INode {
        const res = /^:ref=([a-z0-9 ]+)$/i.exec(text);
        if (res) {
            const name = res[1];
            return new RefNode(data, name);
        }
        return parser.ParseTags(data, text, scope, TagParseContext.Block, line, column);
    }
}
