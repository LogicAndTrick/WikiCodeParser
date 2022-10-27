import { Parser } from '..';
import { HtmlNode } from '../Nodes/HtmlNode';
import { INode } from '../Nodes/INode';
import { ParseData } from '../ParseData';
import { State } from '../State';
import { Tag } from './Tag';

class SpoilerNode implements INode {
    private VisibleText: string;
    private SpoilerContent: INode;

    constructor(visibleText: string, spoilerContent: INode) {
        this.VisibleText = visibleText;
        this.SpoilerContent = spoilerContent;
    }

    ToHtml(): string {
        return this.SpoilerContent.ToHtml();
    }

    ToPlainText(): string {
        return `[${this.VisibleText}](spoiler text)`;
    }

    GetChildren(): INode[] {
        return [this.SpoilerContent];
    }

    ReplaceChild(i: number, node: INode): void {
        if (i != 0) throw new Error('Argument out of range');
        this.SpoilerContent = node;
    }

    HasContent(): boolean {
        return true;
    }
}

export class SpoilerTag extends Tag {
    constructor() {
        super();
        this.Token = 'spoiler';
        this.Element = 'span';
        this.ElementClass = 'spoiler';
        this.MainOption = 'text';
        this.Options = ['text'];
        this.AllOptionsInMain = true;
    }

    public FormatResult(parser: Parser, data: ParseData, state: State, scope: string, options: Record<string, string>, text: string): INode {
        let visibleText = 'Spoiler';
        if (options['text'] && options['text'].length > 0) visibleText = options['text'];

        let before = `<${this.Element}`;
        if (this.ElementClass != null) before += ' class="' + this.ElementClass + '"';
        before += ` title="${visibleText}">`;
        const after = `</${this.Element}>`;
        return new HtmlNode(before, new SpoilerNode(visibleText, parser.ParseTags(data, text, scope, this.TagContext())), after);
    }
}
