import { HtmlNode } from '../Nodes/HtmlNode';
import { INode } from '../Nodes/INode';
import { NodeCollection } from '../Nodes/NodeCollection';
import { PlainTextNode } from '../Nodes/PlainTextNode';
import { UnprocessablePlainTextNode } from '../Nodes/UnprocessablePlainTextNode';
import { ParseData } from '../ParseData';
import { Parser } from '../Parser';
import { State } from '../State';
import { IndexOfAny, Trim, TrimEnd } from '../Util';
import { INodeProcessor } from './INodeProcessor';

export class MarkdownTextProcessor implements INodeProcessor {
    public Priority = 10;

    private static Tokens: string[] = Array.from('`*/_~');
    private static OpenTags: string[] = ['<code>', '<strong>', '<em>', '<span class="underline">', '<span class="strikethrough">'];
    private static CloseTags: string[] = ['</code>', '</strong>', '</em>', '</span>', '</span>'];
    private static StartBreakChars: string[] = Array.from('!^()+=[]{}"\'<>?,. \t\r\n');
    private static ExtraEndBreakChars: string[] = Array.from(':;');

    ShouldProcess(node: INode, _scope: string): boolean {
        return node instanceof PlainTextNode && IndexOfAny(node.Text, MarkdownTextProcessor.Tokens) >= 0;
    }

    private static GetTokenIndex(c: string): number {
        return MarkdownTextProcessor.Tokens.indexOf(c);
    }
    private static IsStartBreakChar(c: string): boolean {
        return MarkdownTextProcessor.StartBreakChars.includes(c);
    }
    private static IsEndBreakChar(c: string): boolean {
        return MarkdownTextProcessor.StartBreakChars.includes(c) || MarkdownTextProcessor.ExtraEndBreakChars.includes(c) || MarkdownTextProcessor.Tokens.includes(c);
    }

    private static ParseToken(tracker: number[], state: State): INode | null {
        const tokStart = state.SourcePosition;
        const index = state.Index;
        const token = state.Next();
        const tokenIndex = MarkdownTextProcessor.GetTokenIndex(token);

        // Make sure we're not already in this token
        if (tracker[tokenIndex] != 0) {
            state.Seek(index, true);
            return null;
        }

        const strStart = state.SourcePosition;
        const str = state.ScanTo(token);

        // the contents cannot be empty or contain newlines, and the next char must be the token
        if (state.Next() != token || str.length == 0 || str.indexOf('\n') >= 0) {
            state.Seek(index, true);
            return null;
        }

        // Make sure we can close this token
        const valid =
            (state.Done || MarkdownTextProcessor.IsEndBreakChar(state.Peek(1))) && // end of string or before an end breaker
            TrimEnd(str).length == str.length; // not whitespace previous
        if (!valid) {
            state.Seek(index, true);
            return null;
        }

        tracker[tokenIndex] = 1;

        // code tokens cannot be nested
        let contents: INode;
        if (token == '`') {
            contents = new UnprocessablePlainTextNode(strStart, str);
        } else {
            const toks = MarkdownTextProcessor.ParseTokens(strStart.Line, strStart.Column, tracker, str);
            contents = toks.length == 1 ? toks[0] : new NodeCollection(...toks);
        }

        tracker[tokenIndex] = 0;

        const ret = new HtmlNode(tokStart, MarkdownTextProcessor.OpenTags[tokenIndex], contents, MarkdownTextProcessor.CloseTags[tokenIndex]);
        ret.PlainBefore = token;
        ret.PlainAfter = token;
        return ret;
    }

    private static ParseTokens(line: number, column: number, tracker: number[], text: string): INode[] {
        const ret: INode[] = [];
        const state = new State(line, column, text);
        let plainPos = state.SourcePosition;
        let plain = '';
        let advanceToken = false;
        while (!state.Done) {
            // always advance one token except at the start, or after we find a valid token
            if (advanceToken) plain += state.Next();
            advanceToken = true;

            plain += state.ScanToAny(MarkdownTextProcessor.Tokens);
            if (state.Done) break;

            // Make sure we can start a new token
            const valid =
                (state.Index == 0 || MarkdownTextProcessor.IsStartBreakChar(state.Text[state.Index - 1])) && // start of string or after a start breaker
                state.Index + 1 < text.length && // not end of string
                Trim(state.Text[state.Index + 1]) != ''; // not whitespace next
            if (!valid) continue;

            // Try to parse a token
            const parsed = this.ParseToken(tracker, state);
            if (parsed === null) continue;

            if (plain.length > 0) ret.push(new PlainTextNode(plainPos, plain));
            ret.push(parsed);

            advanceToken = false;
            plain = '';
            plainPos = state.SourcePosition;
        }
        // Return the rest of the text as plain
        plain += state.ScanToEnd();
        if (plain.length > 0) ret.push(new PlainTextNode(plainPos, plain));

        return ret;
    }

    Process(_parser: Parser, _data: ParseData, parentNode: INode | null, node: INode, _scope: string): INode[] {
        const text = (node as PlainTextNode).Text;

        const ret = [];

        const nextIndex = IndexOfAny(text, MarkdownTextProcessor.Tokens, 0);
        if (nextIndex < 0) {
            // Short circuit
            ret.push(node);
            return ret;
        }

        /*
         * Like everything else here, this isn't exactly markdown, but it's close.
         * _underline_
         * /italics/
         * *bold*
         * ~strikethrough~
         * `code`
         * Very simple rules: no newlines, must start/end on a word boundary, code tags cannot be nested
         */

        // pre-condition: start of a line OR one of: !?^()+=[]{}"'<>,. OR whitespace
        // first and last character is NOT whitespace. everything else is fine except for newlines
        // post-condition: end of a line OR one of: !?^()+=[]{}"'<>,.:; OR whitespace

        const tracker: number[] = MarkdownTextProcessor.Tokens.map(() => 0);

        for (const token of MarkdownTextProcessor.ParseTokens(node.Line, node.Column, tracker, text)) {
            ret.push(token);
        }

        return ret;
    }
}
