import { Lines } from './Lines';
import { INode } from './Nodes/INode';
import { NodeCollection } from './Nodes/NodeCollection';
import { NodeExtensions } from './Nodes/NodeExtensions';
import { PlainTextNode } from './Nodes/PlainTextNode';
import { UnprocessablePlainTextNode } from './Nodes/UnprocessablePlainTextNode';
import { ParseData } from './ParseData';
import { ParserConfiguration } from './ParserConfiguration';
import { ParseResult } from './ParseResult';
import { Position } from './Position';
import { INodeProcessor } from './Processors/INodeProcessor';
import { State } from './State';
import { TagParseContext } from './TagParseContext';
import { OrderByDescending, TrimEnd, TrimStart } from './Util';

export class Parser {
    public Configuration: ParserConfiguration;

    constructor(configuration: ParserConfiguration) {
        this.Configuration = configuration;
    }

    public ParseResult(text: string, scope = ''): ParseResult {
        const data = new ParseData();
        let node = this.ParseElements(data, text, scope, 1);
        node = this.RunProcessors(null, node, data, scope);
        const res = new ParseResult();
        res.Content = node;
        return res;
    }

    public ParseElements(data: ParseData, text: string, scope: string, line: number): INode {
        data.CurrentElementDepth++;
        const root = new NodeCollection();

        // Elements are line-based scopes, an element cannot start in the middle of a line.
        // this is fine to do for line numbers as long as the \r characters at the end of the line, which is normal.
        // \r in the middle of a line will cause column positions to be wrong, but column positions are nice-to-haves anyway, so we don't really need to handle that.
        text = text.replace(/\r/g, '');

        const lines = new Lines(line, text);
        const inscope = OrderByDescending(
            this.Configuration.Elements.filter(x => x.InScope(scope)),
            x => x.Priority
        );
        if (data.CurrentElementDepth > this.Configuration.MaxElementDepth) inscope.splice(0, inscope.length);

        let plainStart = lines.SourceLine;
        const plain: string[] = [];

        while (lines.Next()) {
            // Try and find an element for this line
            let matched = false;
            for (const e of inscope) {
                if (!e.Matches(lines)) continue;

                const con = e.Consume(this, data, lines, scope); // found an element, generate the result
                if (con == null) continue; // no result, guess this element wasn't valid after all

                // if we have any plain text, create a node for it
                if (plain.length > 0) {
                    root.Nodes.push(Parser.TrimWhitespace(this.ParseTags(data, plain.join('\n'), scope, TagParseContext.Block, plainStart, 0)));
                    root.Nodes.push(UnprocessablePlainTextNode.NewLine(new Position(plainStart + plain.length - 1, plain[plain.length - 1].length))); // Newline before next element
                }
                plain.splice(0, plain.length);
                plainStart = lines.SourceLine + 1; // plain starts on the next line

                root.Nodes.push(con);
                root.Nodes.push(UnprocessablePlainTextNode.NewLine(new Position(lines.SourceLine, 0))); // Elements always have a newline after
                matched = true;
                break;
            }

            if (!matched) plain.push(lines.Value()); // there wasn't any match, so this line was plain text
        }

        // parse any plain text that might be left
        if (plain.length > 0) root.Nodes.push(Parser.TrimWhitespace(this.ParseTags(data, plain.join('\n'), scope, TagParseContext.Block, plainStart, 0)));

        // Trim off any whitespace nodes at the end
        const shouldTrim = () => {
            if (root.Nodes.length === 0) return false;
            const last = root.Nodes[root.Nodes.length - 1];
            if (!(last instanceof UnprocessablePlainTextNode)) return false;
            return !last.Text || last.Text.trim() == '';
        };
        while (shouldTrim()) {
            root.Nodes.splice(root.Nodes.length - 1, 1);
        }

        Parser.FlattenNestedNodeCollections(root);
        const ret = Parser.TrimWhitespace(root);

        data.CurrentElementDepth--;
        return ret;
    }

    private static TrimStartAndGetPositionChange(line: number, column: number, text: string): { line: number; column: number; text: string } {
        const trimmed = TrimStart(text);
        const wsLines = text.substring(0, text.length - trimmed.length).split('\n');
        const lineOffset = wsLines.length - 1;
        line += lineOffset;
        if (lineOffset > 0) column = wsLines[lineOffset].length;
        else column -= wsLines[lineOffset].length;
        return { line, column, text: trimmed };
    }

    public static TrimWhitespace(node: INode, start = true, end = true): INode {
        const removeNodes: INode[] = [];

        if (start) {
            NodeExtensions.Walk(node, x => {
                if (x instanceof NodeCollection) return true;
                if (x instanceof UnprocessablePlainTextNode && x.Text != null) {
                    const tsp = Parser.TrimStartAndGetPositionChange(x.Line, x.Column, x.Text);
                    x.Line = tsp.line;
                    x.Column = tsp.column;
                    x.Text = tsp.text;
                }
                if (x instanceof PlainTextNode && x.Text != null) {
                    const tsp = Parser.TrimStartAndGetPositionChange(x.Line, x.Column, x.Text);
                    x.Line = tsp.line;
                    x.Column = tsp.column;
                    x.Text = tsp.text;
                }
                if (x.HasContent()) return false;
                if (x instanceof UnprocessablePlainTextNode || x instanceof PlainTextNode) removeNodes.push(x);
                return true;
            });
        }

        if (end) {
            NodeExtensions.WalkBack(node, x => {
                if (x instanceof NodeCollection) return true;
                if (x instanceof UnprocessablePlainTextNode && x.Text != null) x.Text = TrimEnd(x.Text ?? '');
                if (x instanceof PlainTextNode && x.Text != null) x.Text = TrimEnd(x.Text ?? '');
                if (x.HasContent()) return false;
                if (x instanceof UnprocessablePlainTextNode || x instanceof PlainTextNode) removeNodes.push(x);
                return true;
            });
        }

        for (const rem of removeNodes) {
            NodeExtensions.Remove(node, rem);
        }

        return node;
    }

    public ParseTags(data: ParseData, text: string, scope: string, context: TagParseContext, line: number, column: number): INode {
        data.CurrentTagDepth++;

        const state = new State(line, column, text);
        const root = new NodeCollection();
        const inscope = OrderByDescending(
            this.Configuration.Tags.filter(x => x.InScope(scope)),
            x => x.Priority
        );
        if (data.CurrentTagDepth > this.Configuration.MaxTagDepth) inscope.splice(0, inscope.length);

        while (!state.Done) {
            const pos = state.SourcePosition;
            let plain = state.ScanTo('[');
            if (plain && plain != '') {
                const p = new PlainTextNode(pos, plain);
                p.Line = pos.Line;
                p.Column = pos.Column;
                root.Nodes.push(p);
            }
            if (state.Done) break;

            const token = state.GetToken();
            let found = false;

            for (const t of inscope) {
                if (t.Matches(state, token, context)) {
                    const parsed = t.Parse(this, data, state, scope, context);
                    if (parsed != null) {
                        root.Nodes.push(parsed);
                        found = true;
                        break;
                    }
                }
            }

            if (!found) {
                plain = state.Next();
                if (plain && plain != '') {
                    const p = new PlainTextNode(pos, plain);
                    p.Line = pos.Line;
                    p.Column = pos.Column;
                    root.Nodes.push(p);
                }
            }
        }

        data.CurrentTagDepth--;
        return root;
    }

    public static FlattenNestedNodeCollections(node: INode): void {
        if (node instanceof NodeCollection) {
            const coll: NodeCollection = node;
            while (coll.Nodes.some(x => x instanceof NodeCollection)) {
                coll.Nodes = coll.Nodes.flatMap(x => (x instanceof NodeCollection ? x.Nodes : [x]));
            }
        } else {
            const ch = node.GetChildren();
            for (let i = 0; i < ch.length; i++) {
                while (ch[i] instanceof NodeCollection && (ch[i] as NodeCollection).Nodes.length == 1) {
                    const chcoll = ch[i] as NodeCollection;
                    node.ReplaceChild(i, chcoll.Nodes[0]);
                    ch[i] = chcoll.Nodes[0];
                }
            }
        }

        for (const child of node.GetChildren()) {
            Parser.FlattenNestedNodeCollections(child);
        }
    }

    public RunProcessors(parentNode: INode | null, node: INode, data: ParseData, scope: string): INode {
        for (const processor of OrderByDescending(this.Configuration.Processors, x => x.Priority)) {
            node = this.RunProcessor(parentNode, node, processor, data, scope);
        }

        return node;
    }

    public RunProcessor(parentNode: INode | null, node: INode, processor: INodeProcessor, data: ParseData, scope: string): INode {
        // If the node can be processed, don't touch subnodes - the processor can invoke RunProcessor if it's needed.
        if (processor.ShouldProcess(node, scope)) {
            const result = processor.Process(this, data, parentNode, node, scope);
            return result.length == 1 ? result[0] : new NodeCollection(...result);
        }

        const children = node.GetChildren();

        for (let i = 0; i < children.length; i++) {
            const child = children[i];
            const processed = this.RunProcessor(node, child, processor, data, scope);
            node.ReplaceChild(i, processed);
        }

        return node;
    }
}
