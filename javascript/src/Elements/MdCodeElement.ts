import { Parser } from '..';
import { Lines } from '../Lines';
import { HtmlNode } from '../Nodes/HtmlNode';
import { INode } from '../Nodes/INode';
import { UnprocessablePlainTextNode } from '../Nodes/UnprocessablePlainTextNode';
import { ParseData } from '../ParseData';
import { Element } from './Element';
import { PreElement } from './PreElement';

export class MdCodeElement extends Element {
    constructor() {
        super();
        this.Priority = 10;
    }

    public Matches(lines: Lines): boolean {
        const value = lines.Value();
        return value.startsWith('```');
    }

    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    public Consume(parser: Parser, data: ParseData, lines: Lines, _scope: string): INode {
        const current = lines.Current();
        let firstLine = lines.Value().substring(3).trimEnd();

        let lang: string | null = null;
        if (PreElement.AllowedLanguages.includes(firstLine.toLowerCase())) {
            lang = firstLine;
            firstLine = '';
        }

        let arr = [firstLine];

        let found = false;
        while (lines.Next()) {
            const value = lines.Value().trimEnd();
            if (value.endsWith('```')) {
                const lastLine = value.substring(0, value.length - 3);
                arr.push(lastLine);
                found = true;
                break;
            } else {
                arr.push(value);
            }
        }

        if (!found) {
            lines.SetCurrent(current);
            return null;
        }

        // Trim blank lines from the start and end of the array
        for (let i = 0; i < 2; i++) {
            while (arr.length > 0 && arr[0].trim() == '') arr.splice(0, 1);
            arr.reverse();
        }

        // Replace all tabs with 4 spaces
        arr = arr.map(x => x.replace(/\t/g, '    '));

        // Find the longest common whitespace amongst all lines (ignore blank lines)
        const longestWhitespace = arr.reduce((c, i) => {
            if (i.trim().length == 0) return c;
            const wht = i.length - i.trimStart().length;
            return Math.min(wht, c);
        }, 9999);

        // Dedent all lines by the longest common whitespace
        arr = arr.map(a => a.substring(Math.min(longestWhitespace, a.length)));

        const plain = new UnprocessablePlainTextNode(arr.join('\n'));
        const cls = !lang || lang.trim() == '' ? '' : ` class="lang-${lang}"`;
        const before = `<pre${cls}><code>`;
        const after = '</code></pre>';
        return new HtmlNode(before, plain, after);
    }
}