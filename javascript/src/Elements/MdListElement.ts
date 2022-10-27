import { Parser } from '..';
import { Lines } from '../Lines';
import { INode } from '../Nodes/INode';
import { NodeCollection } from '../Nodes/NodeCollection';
import { PlainTextNode } from '../Nodes/PlainTextNode';
import { ParseData } from '../ParseData';
import { TagParseContext } from '../TagParseContext';
import { Element } from './Element';

class ListNode implements INode {
    public Tag: string;
    public Items: ListItemNode[];
    constructor(tag: string, ...items: ListItemNode[]) {
        this.Tag = tag;
        this.Items = items;
    }
    ToHtml(): string {
        let sb = '';
        sb += `<${this.Tag}>\n`;
        for (const item of this.Items) sb += item.ToHtml();
        sb += `</${this.Tag}>\n`;
        return sb;
    }
    ToPlainText(): string {
        return this.ToPlainTextPrefixed('');
    }
    public ToPlainTextPrefixed(prefix: string): string {
        const st = prefix + (this.Tag == 'ol' ? '#' : '-');
        let sb = '';
        for (const item of this.Items) sb += item.ToPlainTextPrefixed(st);
        return sb;
    }
    GetChildren(): INode[] {
        return [...this.Items];
    }
    ReplaceChild(i: number, node: INode): void {
        this.Items[i] = node as ListItemNode;
    }
    HasContent(): boolean {
        return true;
    }
}

class ListItemNode implements INode {
    public Content: INode;
    public Subtrees: ListNode[];
    constructor(content: INode) {
        this.Content = content;
        this.Subtrees = [];
    }
    ToHtml(): string {
        let sb = '<li>';
        sb += this.Content.ToHtml();
        for (const st of this.Subtrees) sb += st.ToHtml();
        sb += '</li>\n';
        return sb;
    }
    ToPlainText(): string {
        throw new Error('Invalid operation');
    }
    public ToPlainTextPrefixed(prefix: string): string {
        let sb = prefix + ' ';
        sb += this.Content.ToPlainText() + '\n';
        for (const st of this.Subtrees) sb += st.ToPlainTextPrefixed(prefix);
        return sb;
    }
    GetChildren(): INode[] {
        return [this.Content, ...this.Subtrees];
    }
    ReplaceChild(i: number, node: INode): void {
        if (i == 0) this.Content = node;
        else this.Subtrees[i - 1] = node as ListNode;
    }
    HasContent(): boolean {
        return true;
    }
}

export class MdListElement extends Element {
    private static UlTokens = new Set<string>(['*', '-']);
    private static OlTokens = new Set<string>(['#']);

    private static IsUnsortedToken(c: string) {
        return MdListElement.UlTokens.has(c);
    }

    private static IsSortedToken(c: string) {
        return MdListElement.OlTokens.has(c);
    }

    private static IsListToken(c: string) {
        return MdListElement.IsUnsortedToken(c) || MdListElement.IsSortedToken(c);
    }

    private static IsValidListItem(value: string, currentLevel: number) {
        const len = value.length;
        if (len == 0) return 0;

        let tokens = 0;
        let foundSpace = false;
        for (let i = 0; i < len; i++) {
            const c = value[i];
            if (MdListElement.IsListToken(c)) {
                tokens++;
                continue;
            }

            if (c == ' ') {
                foundSpace = true;
                break;
            }

            return 0;
        }

        if (foundSpace && tokens > 0 && tokens <= currentLevel + 1) return tokens;
        return 0;
    }

    public Matches(lines: Lines): boolean {
        const value = lines.Value().trim();
        return MdListElement.IsValidListItem(value, 0) > 0;
    }

    public Consume(parser: Parser, data: ParseData, lines: Lines, scope: string): INode {
        const current = lines.Current();

        // Put all the subtrees into a dummy item node
        const item = new ListItemNode(PlainTextNode.Empty());
        this.CreateListItems(item, '', parser, data, lines, scope);

        if (item.Subtrees.length == 0) {
            lines.SetCurrent(current);
            return null;
        }

        // Pull the subtrees out again for the result
        if (item.Subtrees.length == 1) return item.Subtrees[0];
        return new NodeCollection(...item.Subtrees);
    }

    private CreateListItems(lastItemNode: ListItemNode, prefix: string, parser: Parser, data: ParseData, lines: Lines, scope: string): ListItemNode[] {
        const ret: ListItemNode[] = [];
        do {
            let value = lines.Value().trimEnd();

            if (!value.startsWith(prefix)) {
                // No longer valid for this list
                lines.Back();
                break;
            }

            value = value.substring(prefix.length); // strip the prefix off the line

            // Possibilities:
            // empty string : not valid - stop parsing
            // first character is whitespace : trim and create list item
            // first character is list token, second character is whitespace: create sublist
            // anything else : not valid - stop parsing

            if (value.length > 1 && value[0] == ' ' && prefix.length > 0) { // don't allow this if we're parsing at level 0
                // List item
                value = value.trimStart();

                // Support for continuations
                while (value.endsWith('^')) {
                    if (value.endsWith('\\^')) // super basic way to escape continuations
                    {
                        value = value.substring(0, value.length - 2) + '^';
                        break;
                    }
                    else if (lines.Next()) {
                        value = value.substring(0, value.length - 1).trim() + '\n' + lines.Value().trimStart();
                    }
                    else {
                        break;
                    }
                }

                const pt = parser.ParseTags(data, value.trim(), scope, TagParseContext.Block);
                lastItemNode = new ListItemNode(pt);
                ret.push(lastItemNode);
            } else if (value.length > 2 && MdListElement.IsListToken(value[0]) && value[1] == ' ' && lastItemNode != null) {
                // Sublist
                const tag = MdListElement.IsSortedToken(value[0]) ? 'ol' : 'ul';
                const sublist = new ListNode(tag, ...this.CreateListItems(lastItemNode, prefix + value[0], parser, data, lines, scope));
                lastItemNode.Subtrees.push(sublist);
            } else {
                // Cannot parse this line, list is complete
                lines.Back();
                break;
            }
        } while (lines.Next());
        return ret;
    }
}
