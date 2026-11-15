import { HtmlHelper } from '../HtmlHelper';
import { Position } from '../Position';
import { INode } from './INode';

export class UnprocessablePlainTextNode implements INode {
    public static Empty(pos: Position): INode {
        return new UnprocessablePlainTextNode(pos, '');
    }
    public static NewLine(pos: Position): INode {
        return new UnprocessablePlainTextNode(pos, '\n');
    }

    public Line: number = 0;
    public Column: number = 0;
    public Text: string;

    constructor(pos: Position, text: string) {
        this.Line = pos.Line;
        this.Column = pos.Column;
        this.Text = text;
    }

    ToHtml(): string {
        return HtmlHelper.Encode(this.Text);
    }
    ToPlainText(): string {
        return this.Text;
    }
    GetChildren(): INode[] {
        return [];
    }

    ReplaceChild(_i: number, _node: INode): void {
        throw new Error('Invalid operation');
    }
    HasContent(): boolean {
        return !!this.Text && this.Text.trim() != '';
    }
}
