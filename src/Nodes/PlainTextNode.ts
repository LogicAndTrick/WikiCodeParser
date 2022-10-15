import { HtmlHelper } from '../HtmlHelper';
import { INode } from './INode';

export class PlainTextNode implements INode {

    public static Empty() : INode { return new PlainTextNode(''); }

    public Text : string;

    constructor(text : string) {
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
    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    ReplaceChild(_i: number, _node: INode): void {
        throw new Error('Invalid operation');
    }
    HasContent(): boolean {
        return this.Text && this.Text.trim() != '';
    }
}
