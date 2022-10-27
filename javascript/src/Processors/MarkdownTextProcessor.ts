import { Parser } from '..';
import { HtmlNode } from '../Nodes/HtmlNode';
import { INode } from '../Nodes/INode';
import { NodeCollection } from '../Nodes/NodeCollection';
import { PlainTextNode } from '../Nodes/PlainTextNode';
import { UnprocessablePlainTextNode } from '../Nodes/UnprocessablePlainTextNode';
import { ParseData } from '../ParseData';
import { IndexOfAny } from '../Util';
import { INodeProcessor } from './INodeProcessor';

type endPositionOutobject = { endPosition: number };

export class MarkdownTextProcessor implements INodeProcessor {
    public Priority = 10;

    private static Tokens: string[] = Array.from('`*/_~');
    private static OpenTags: string[] = ['<code>', '<strong>', '<em>', '<span class="underline">', '<span class="strikethrough">'];
    private static CloseTags: string[] = ['</code>', '</strong>', '</em>', '</span>', '</span>'];
    private static StartBreakChars: string[] = Array.from('!^()+=[]{}"\'<>?,. \t\r\n');
    private static ExtraEndBreakChars: string[] = Array.from(':;');

    // eslint-disable-next-line @typescript-eslint/no-unused-vars
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

    private static ParseToken(tracker: number[], text: string, position: number, endPositionObj: endPositionOutobject): INode {
        endPositionObj.endPosition = -1;
        const token = text[position];
        const tokenIndex = MarkdownTextProcessor.GetTokenIndex(token);

        // Make sure we're not already in this token
        if (tracker[tokenIndex] != 0) return null;

        const endToken = text.indexOf(token, position + 1);
        if (endToken <= position + 1) return null;
        if (text.substring(position, endToken).indexOf('\n') >= 0) return null; // no newlines

        // Make sure we can close this token
        const valid = (endToken + 1 == text.length || MarkdownTextProcessor.IsEndBreakChar(text[endToken + 1])) // end of string or before an end breaker
            && text[endToken - 1].trim() != ''; // not whitespace previous
        if (!valid) return null;

        const str = text.substring(position + 1, endToken);

        tracker[tokenIndex] = 1;

        // code tokens cannot be nested
        let contents: INode;
        if (token == '`') {
            contents = new UnprocessablePlainTextNode(str);
        } else {
            const toks = MarkdownTextProcessor.ParseTokens(tracker, str);
            contents = toks.length == 1 ? toks[0] : new NodeCollection(...toks);
        }

        tracker[tokenIndex] = 0;

        endPositionObj.endPosition = endToken;

        return new HtmlNode(MarkdownTextProcessor.OpenTags[tokenIndex], contents, MarkdownTextProcessor.CloseTags[tokenIndex]);
    }

    private static ParseTokens(tracker: number[], text: string): INode[] {
        const ret = [];
        let plainStart = 0;
        let index = 0;
        // eslint-disable-next-line no-constant-condition
        while (true) {
            const nextIndex = IndexOfAny(text, MarkdownTextProcessor.Tokens, index);
            if (nextIndex < 0) break;

            // Make sure we can start a new token
            const valid = (nextIndex == 0 || MarkdownTextProcessor.IsStartBreakChar(text[nextIndex - 1])) // start of string or after a start breaker
                && nextIndex + 1 < text.length // not end of string
                && text[nextIndex + 1].trim() != ''; // not whitespace next
            if (!valid) {
                index = nextIndex + 1;
                continue;
            }

            const endIndexObj: endPositionOutobject = { endPosition: -1 };
            const parsed = MarkdownTextProcessor.ParseToken(tracker, text, nextIndex, endIndexObj);
            if (parsed == null) {
                index = nextIndex + 1; // no match, skip this token
            }
            else {
                if (plainStart < nextIndex) ret.push(new PlainTextNode(text.substring(plainStart, nextIndex)));
                ret.push(parsed);
                index = plainStart = endIndexObj.endPosition + 1;
            }
        }
        // Return the rest of the text as plain
        if (plainStart < text.length) ret.push(new PlainTextNode(text.substring(plainStart)));

        return ret;
    }

    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    Process(_parser: Parser, _data: ParseData, node: INode, _scope: string): INode[] {
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

        for (const token of MarkdownTextProcessor.ParseTokens(tracker, text)) {
            ret.push(token);
        }

        return ret;
    }
}
