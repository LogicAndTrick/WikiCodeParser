import { Parser } from '..';
import { Lines } from '../Lines';
import { HtmlNode } from '../Nodes/HtmlNode';
import { INode } from '../Nodes/INode';
import { NodeCollection } from '../Nodes/NodeCollection';
import { ParseData } from '../ParseData';
import { Element } from './Element';

class ColumnNode implements INode {
    public Width: number;
    public Content: INode;
    constructor(width: number, content: INode) {
        this.Width = width;
        this.Content = content;
    }
    ToHtml(): string {
        return `<div class="col-md-${this.Width}">\n${this.Content.ToHtml()}</div>\n`;
    }
    ToPlainText(): string {
        return this.Content.ToPlainText();
    }
    GetChildren(): INode[] {
        return [this.Content];
    }
    ReplaceChild(i: number, node: INode): void {
        if (i != 0) throw new Error('Argument out of range');
        this.Content = node;
    }
    HasContent(): boolean {
        return true;
    }
}

export class MdColumnsElement extends Element {
    public Matches(lines: Lines): boolean {
        const value = lines.Value();
        return value.startsWith('%%columns=');
    }
    public Consume(parser: Parser, data: ParseData, lines: Lines, scope: string): INode {
        const current = lines.Current();

        const meta = lines.Value().substring(10);
        const colDefs = meta.split(':').map(x => parseInt(x, 10) ?? 0);
        let total = 0;

        for (const d of colDefs) {
            if (d > 0) {
                total += d;
            } else {
                lines.SetCurrent(current);
                return null;
            }
        }

        if (total != 12) {
            lines.SetCurrent(current);
            return null;
        }

        let i = 0;

        let arr: string[] = [];
        const cols: ColumnNode[] = [];
        while (lines.Next() && i < colDefs.length) {
            const value = lines.Value().trimEnd();
            if (value == '%%') {
                cols.push(new ColumnNode(colDefs[i], parser.ParseElements(data, arr.join('\n'), scope)));
                arr = [];
                i++;
            } else {
                arr.push(value);
            }
            if (i >= colDefs.length) break;
        }

        if (i != colDefs.length || arr.length > 0) {
            lines.SetCurrent(current);
            return null;
        }

        return new HtmlNode('<div class="row">', new NodeCollection(...cols), '</div>');
    }
}