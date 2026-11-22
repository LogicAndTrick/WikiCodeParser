import { HtmlHelper } from '../HtmlHelper';
import { HtmlNode } from '../Nodes/HtmlNode';
import { HtmlNodeBuilder } from '../Nodes/HtmlNodeBuilder';
import { INode } from '../Nodes/INode';
import { NodeExtensions } from '../Nodes/NodeExtensions';
import { PlainTextNode } from '../Nodes/PlainTextNode';
import { UnprocessablePlainTextNode } from '../Nodes/UnprocessablePlainTextNode';
import { ParseData } from '../ParseData';
import { Parser } from '../Parser';
import { State } from '../State';
import { INodeProcessor } from './INodeProcessor';

export class AutoLinkingProcessor implements INodeProcessor {
    public Priority = 11;

    ShouldProcess(node: INode, _scope: string): boolean {
        // handle <a> parent nodes so we skip processing of everything under them
        if (node instanceof HtmlNode && node.HtmlBefore.includes('<a ')) return true;
        return node instanceof PlainTextNode && (node.Text.toLowerCase().includes('http') || node.Text.includes('@'));
    }

    private static AllowedStartChars: Set<string> = new Set<string>([' ', '\n', '\r', '\t', '(', '"']);
    private static AllowedEndChars: Set<string> = new Set<string>([' ', '\n', '\r', '\t', ')', '"', '.', ',', '!', '?']);

    Process(parser: Parser, data: ParseData, parentNode: INode | null, node: INode, _scope: string): INode[] {
        if (!(node instanceof PlainTextNode)) {
            return [node];
        }

        const text = node.Text;
        const state = new State(node.Line, node.Column, text); // just for tracking newlines

        const ret: INode[] = [];

        const allMatches: RegExpExecArray[] = [];

        const urlMatcher = /(?<![^\s""(])(?<url>https?:\/\/[^\][""\s]+)(?=[.,!?""]{0,5}(?!\S))/gi;
        let urlMatches = urlMatcher.exec(text);
        while (urlMatches != null) {
            allMatches.push(urlMatches);
            urlMatches = urlMatcher.exec(text);
        }

        const emailMatcher = /(?<![^\s""(])(?<email>[^\][""\s@]+@[^\][""\s@.]+(?:\.[^\][""\s@.]+)+)(?=[.,!?""]{0,5}(?!\S))/gi;
        let emailMatches = emailMatcher.exec(text);
        while (emailMatches != null) {
            allMatches.push(emailMatches);
            emailMatches = emailMatcher.exec(text);
        }

        allMatches.sort((a, b) => a.index - b.index);

        let start = 0;
        for (const urlMatch of allMatches) {
            const idx = urlMatch.index;
            let len = urlMatch[0].length;
            if (idx < start) continue;

            // skip matches starting on the first letter if the previous sibling doesn't end in whitespace or one of the allowed punctuation chars
            if (idx == 0 && !NodeExtensions.PreviousNodeEndsWith(node, parentNode, AutoLinkingProcessor.AllowedStartChars)) continue;

            // walk back any punctuation or brackets if they are balanced
            while (len > 0 && AutoLinkingProcessor.ShouldWalkBack(text, idx, len)) len--;

            if (len < 5) continue; // urls and emails can't be this short. something is wrong, so skip it.

            // skip matches ending on the last letter if the next sibling doesn't start in whitespace or one of the allowed punctuation chars
            if (idx + len == text.length && !NodeExtensions.NextNodeStartsWith(node, parentNode, AutoLinkingProcessor.AllowedEndChars)) continue;

            const matchingText = text.substring(idx, idx + len);
            if (urlMatch.groups && urlMatch.groups['url'] && !HtmlHelper.ValidateUrl(matchingText)) continue;

            // return the prefix text
            if (idx > start) {
                state.Seek(start, true);
                ret.push(new PlainTextNode(state.SourcePosition, text.substring(start, urlMatch.index)));
            }
            if (urlMatch.groups && urlMatch.groups['url']) {
                state.Seek(idx, true);
                ret.push(
                    HtmlNodeBuilder.make('a', state.SourcePosition)
                        .attr('href', HtmlHelper.SanitiseUrl(matchingText))
                        .add(new UnprocessablePlainTextNode(state.SourcePosition, matchingText))
                        .toNode()
                );
            } else if (urlMatch.groups && urlMatch.groups['email']) {
                state.Seek(idx, true);
                ret.push(
                    HtmlNodeBuilder.make('a', state.SourcePosition)
                        .attr('href', `mailto:${matchingText}`)
                        .add(new UnprocessablePlainTextNode(state.SourcePosition, matchingText))
                        .toNode()
                );
            }
            start = idx + len;
        }

        // return the remaining text
        if (start < text.length) {
            state.Seek(start, true);
            ret.push(new PlainTextNode(state.SourcePosition, text.substring(start)));
        }

        return ret;
    }

    private static Punctuation = new Set<string>(['.', '!', '?', ',']);
    private static UrlFragments = new Set<string>(['/', '#', '?', '&', ':']);

    private static ShouldWalkBack(text: string, idx: number, len: number): boolean {
        let c = text[idx + len - 1]; // last char of the match
        if (AutoLinkingProcessor.Punctuation.has(c)) return true; // english punctuation, unlikely a url ends with these

        // if we get a close paren, then try to balance them
        if (c === ')') {
            let lvl = 0;
            do {
                c = text[idx + len - 1];
                if (c === ')') lvl++;
                else if (c === '(') lvl--;
                else if (AutoLinkingProcessor.UrlFragments.has(c)) break; // url/route fragment, assume it can't be balanced
                len--;
            } while (len > 0 && lvl !== 0);
            if (lvl > 0) return true;
        }
        return false;
    }
}
