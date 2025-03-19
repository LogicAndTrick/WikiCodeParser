import { Parser } from '..';
import { Lines } from '../Lines';
import { HtmlNode } from '../Nodes/HtmlNode';
import { INode } from '../Nodes/INode';
import { NodeCollection } from '../Nodes/NodeCollection';
import { ParseData } from '../ParseData';
import { TagParseContext } from '../TagParseContext';
import { Element } from './Element';

export class QuoteElement extends Element {
    private static OpenQuote = /^\[quote(?:(?: name)?=([^\]]*))?\]/i;
    private static CloseQuoteLength = 8;

    Matches(lines: Lines): boolean {
        const value = lines.Value().trim();
        return value.length > 6 && value.toLowerCase().startsWith("[quote") && QuoteElement.OpenQuote.test(value);
    }

    public Consume(parser: Parser, data: ParseData, lines: Lines, scope: string): INode {
        const current = lines.Current();

        let line = lines.Value().trim();
        const res = QuoteElement.OpenQuote.exec(line);
        if (!res) {
            lines.SetCurrent(current);
            return null;
        }

        const { text, author, postfix } = QuoteElement.BalanceQuotes(lines);
        if (!text) {
            lines.SetCurrent(current);
            return null;
        }

        let before = "<blockquote>";
        let plainBefore = "[quote]\n";
        if (author) {
            before += `<strong class="quote-name">${author} said:</strong><br/>`;
            plainBefore = `${author} said: ${plainBefore}`;
        }

        const node = new HtmlNode(before, parser.ParseElements(data, text, scope), "</blockquote>");
        node.PlainBefore = plainBefore;
        node.PlainAfter = "\n[/quote]";
        node.IsBlockNode = true;

        if (postfix) {
            return new NodeCollection(node, parser.ParseTags(data, postfix, scope, TagParseContext.Inline));
        }
        return node;
    }

    public static BalanceQuotes(lines: Lines): { text: string | null, author: string | null, postfix: string | null } {
        let name = null;
        let postfix = null;
        
        const openQuote = new RegExp(QuoteElement.OpenQuote, 'iy');

        let line = lines.Value().trimStart();
        let openMat = openQuote.exec(line);
        if (!openMat) return { text: null, author: name, postfix };

        if (openMat[1]) name = openMat[1];

        line = line.substring(openMat[0].length);
        const arr = [];
        let currentLevel = 1;

        do {
            let idx = 0;
            do {
                openQuote.lastIndex = idx;
                openMat = openQuote.exec(line);
                const openMatIdx = openMat ? openMat.index : -1;
                const closeMatIdx = line.toLowerCase().indexOf("[/quote]", idx);

                if (openMatIdx >= 0 && (closeMatIdx < 0 || closeMatIdx > openMatIdx)) {
                    // Open quote
                    currentLevel++;
                    idx = openMat.index + openMat[0].length;
                } else if (closeMatIdx >= 0) {
                    // Close quote
                    currentLevel--;
                    if (currentLevel === 0) {
                        if (line.length > closeMatIdx + QuoteElement.CloseQuoteLength) postfix = line.substring(closeMatIdx + QuoteElement.CloseQuoteLength);
                        arr.push(line.substring(0, closeMatIdx));
                        return { text: arr.join("\n"), author: name, postfix };
                    }
                    idx = closeMatIdx + QuoteElement.CloseQuoteLength;
                } else {
                    arr.push(line);
                    break;
                }
            } while (true);

            if (lines.Next()) line = lines.Value();
            else break;
        } while (true);

        return { text: null, author: name, postfix };
    }
}
