import { HtmlHelper } from '../HtmlHelper';
import { HtmlNode } from '../Nodes/HtmlNode';
import { INode } from '../Nodes/INode';
import { PlainTextNode } from '../Nodes/PlainTextNode';
import { ParseData } from '../ParseData';
import { Parser } from '../Parser';
import { INodeProcessor } from './INodeProcessor';

export class AutoLinkingProcessor implements INodeProcessor {
    public Priority = 9;

    ShouldProcess(node: INode, _scope: string): boolean {
        return node instanceof PlainTextNode && (node.Text.toLowerCase().includes('http') || node.Text.includes('@'));
    }

    Process(parser: Parser, data: ParseData, node: INode, _scope: string): INode[] {
        const text = (node as PlainTextNode).Text;

        const ret: INode[] = [];

        const allMatches: RegExpExecArray[] = [];

        const urlMatcher = /(?<!\S)(?<url>https?:\/\/[^\][""\s]+)(?!\S)/gi;
        let urlMatch = urlMatcher.exec(text);
        while (urlMatch != null) {
            allMatches.push(urlMatch);
            urlMatch = urlMatcher.exec(text);
        }

        const emailMatcher = /(?<!\S)(?<email>[^\][""\s@]+@[^\][""\s@.]+(?:\.[^\][""\s@.]+)+)(?=\.?(?!\S))/gi;
        let emailMatch = emailMatcher.exec(text);
        while (emailMatch != null) {
            allMatches.push(emailMatch);
            emailMatch = emailMatcher.exec(text);
        }

        allMatches.sort((a, b) => a.index - b.index);

        let start = 0;
        for (const urlMatch of allMatches) {
            if (urlMatch.index < start) continue;
            if (urlMatch.index > start) ret.push(new PlainTextNode(text.substring(start, urlMatch.index)));
            if (urlMatch.groups && urlMatch.groups['url']) {
                const url = urlMatch.groups['url'];
                ret.push(new HtmlNode(`<a href="${HtmlHelper.AttributeEncode(url)}">`, new PlainTextNode(url), '</a>'));
            } else if (urlMatch.groups && urlMatch.groups['email']) {
                const email = urlMatch.groups['email'];
                ret.push(new HtmlNode(`<a href="mailto:${HtmlHelper.AttributeEncode(email)}">`, new PlainTextNode(email), '</a>'));
            }
            start = urlMatch.index + urlMatch[0].length;
        }
        if (start < text.length) ret.push(new PlainTextNode(text.substring(start)));

        return ret;
    }
}
