import { Parser } from '..';
import { Lines } from '../Lines';
import { HtmlNode } from '../Nodes/HtmlNode';
import { INode } from '../Nodes/INode';
import { NodeCollection } from '../Nodes/NodeCollection';
import { RefNode } from '../Nodes/RefNode';
import { ParseData } from '../ParseData';
import { TagParseContext } from '../TagParseContext';
import { Element } from './Element';

class TableRow implements INode {
    public Type: string;
    public Cells: INode[];

    constructor(type: string, ...cells: INode[]) {
        this.Type = type;
        this.Cells = cells;
    }

    ToHtml(): string {
        let sb = '<tr>\n';
        for (const cell of this.Cells) {
            sb += `<${this.Type}>${cell.ToHtml()}</${this.Type}>\n`;
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
        this.Cells[i] = node;
    }

    HasContent(): boolean {
        return true;
    }
}

export class MdTableElement extends Element {
    public Matches(lines: Lines): boolean {
        const value = lines.Value().trimEnd();
        return value.length >= 2 && value[0] == '|' && (value[1] == '=' || value[1] == '-');
    }
    public Consume(parser: Parser, data: ParseData, lines: Lines, scope: string): INode {
        const arr: TableRow[] = [];
        do {
            const value = lines.Value().trimEnd();
            if (value.length < 2 || value[0] != '|' || (value[1] != '=' && value[1] != '-')) {
                lines.Back();
                break;
            }
            const cells = MdTableElement.SplitTable(value.substring(2)).map(x => MdTableElement.ResolveCell(x, parser, data, scope));
            arr.push(new TableRow(value[1] == '=' ? 'th' : 'td', ...cells));
        } while (lines.Next());

        return new HtmlNode('<table class="table table-bordered">', new NodeCollection(...arr), '</table>');
    }

    private static SplitTable(text: string): string[] {
        const ret = [];
        let level = 0;
        let last = 0;
        text = text.trim();
        const len = text.length;
        let i: number;
        for (i = 0; i < len; i++) {
            const c = text[i];
            if (c == '[') level++;
            else if (c == ']') level--;
            else if ((c == '|' && level == 0) || i == len - 1) {
                ret.push(text.substring(last, i + (i == len - 1 ? 1 : 0)).trim());
                last = i + 1;
            }
        }
        if (last < len) ret.push(text.substring(last, i + (i == len - 1 ? 1 : 0)).trim());
        return ret;
    }

    private static ResolveCell(text: string, parser: Parser, data: ParseData, scope: string): INode {
        const res = /^:ref=([a-z0-9 ]+)$/i.exec(text.trim());
        if (res) {
            const name = res[1];
            return new RefNode(data, name);
        }
        return parser.ParseTags(data, text, scope, TagParseContext.Block);
    }
}
