import { Parser } from '..';
import { Colours } from '../Colours';
import { Lines } from '../Lines';
import { HtmlNode } from '../Nodes/HtmlNode';
import { INode } from '../Nodes/INode';
import { UnprocessablePlainTextNode } from '../Nodes/UnprocessablePlainTextNode';
import { ParseData } from '../ParseData';
import { Element } from './Element';

export class PreElement extends Element {

    private static AllowedLanguages: string[] = [
        'php', 'dos', 'bat', 'cmd', 'css', 'cpp', 'c', 'c++', 'cs', 'ini', 'json', 'xml', 'html', 'angelscript',
        'javascript', 'js', 'plaintext'
    ];

    public Matches(lines: Lines): boolean {
        const value = lines.Value().trim();
        return value.length > 4 && value.startsWith('[pre') && value.match(/\[pre(=[a-z ]+)?\]/i) != null;
    }
    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    public Consume(parser: Parser, data: ParseData, lines: Lines, _scope: string): INode {
        const current = lines.Current();
        let arr: string[] = [];

        let line = lines.Value().trim();
        const res = line.match(/\[pre(?:=([a-z0-9 ]+))?\]/i);
        if (!res) {
            lines.SetCurrent(current);
            return null;
        }

        line = line.substring(res[0].length);

        let lang: string | undefined = undefined;
        let hl = false;
        if (res[1]) {
            const spl = res[1].split(' ');
            hl = spl.includes('highlight');
            lang = spl.find(x => x != 'highlight')?.toLowerCase();
            if (!PreElement.AllowedLanguages.includes(lang)) lang = undefined;
        }

        if (line.endsWith('[/pre]')) {
            arr.push(line.substring(0, line.length - 6));
        } else {
            if (line.length > 0) arr.push(line);
            let found = false;
            while (lines.Next()) {
                const value = lines.Value().trimEnd();
                if (value.endsWith('[/pre]')) {
                    const lastLine = value.substring(0, value.length - 6);
                    arr.push(lastLine);
                    found = true;
                    break;
                } else {
                    arr.push(value);
                }
            }

            if (!found || arr.length == 0) {
                lines.SetCurrent(current);
                return null;
            }
        }

        // Trim blank lines from the start and end of the array
        for (let i = 0; i < 2; i++) {
            while (arr.length > 0 && arr[0].trim() == '') arr.splice(0, 1);
            arr.reverse();
        }

        // Process highlight commands
        type HighlightCommand = { firstLine: number, numLines: number, color: string };
        let highlight: HighlightCommand[] = [];
        if (hl) {
            // Highlight commands get their own line so we need to keep track of which lines we're removing as we go
            const newArr: string[] = [];
            let firstLine = 0;
            for (const srcLine of arr) {
                if (srcLine.startsWith('@@')) {
                    const match = srcLine.match(/^@@(?:(#[0-9a-f]{3}|#[0-9a-f]{6}|[a-z]+|\d+)(?::(\d+))?)?$/im);
                    if (match != null) {
                        let numLines = 1;
                        let color = '#FF8000';
                        for (let i = 1; i < match.length; i++) {
                            const p = match[i];
                            if (Colours.IsValidColor(p)) color = p;
                            else if (parseInt(p, 10)) numLines = parseInt(p, 10);
                        }
                        highlight.push({ firstLine, numLines, color });
                        continue;
                    }
                }
                firstLine++;
                newArr.push(srcLine);
            }
            arr = newArr;

            // Make sure highlights don't overlap each other or go past the end of the block
            highlight.push({ firstLine: arr.length, numLines: 0, color: '' });
            for (let i = 0; i < highlight.length - 1; i++) {
                const { firstLine: currFirst, numLines: currNum, color: currCol } = highlight[i];
                const { firstLine: nextFirst } = highlight[i + 1];
                const lastLine = currFirst + currNum - 1;
                if (lastLine >= nextFirst) highlight[i] = { firstLine: currFirst, numLines: nextFirst - currFirst, color: currCol };
            }
            highlight = highlight.filter(x => x.numLines > 0);
        }

        arr = PreElement.FixCodeIndentation(arr);

        const highlights = highlight
            .map(h => `<div class="line-highlight" style="top: ${h.firstLine}em; height: ${h.numLines}em; background: ${h.color};"></div>`)
            .join('');
        const plain = new UnprocessablePlainTextNode(arr.join('\n'));
        const cls = !lang || lang.trim() == '' ? '' : ` class="lang-${lang}"`;
        const before = `<pre${cls}><code>${highlights}`;
        const after = '</code></pre>';
        return new HtmlNode(before, plain, after);
    }

    public static FixCodeIndentation(arr: string[]): string[] {
        // Replace all tabs with 4 spaces
        arr = arr.map(x => x.replace(/\t/g, '    '));

        // Find the longest common whitespace amongst all lines (ignore blank lines)
        const longestWhitespace = arr.reduce((c, i) => {
            if (i.trim().length == 0) return c;
            const wht = i.length - i.trimStart().length;
            return Math.min(wht, c);
        }, 9999);

        // Dedent all lines by the longest common whitespace
        return arr.map(a => a.substring(Math.min(longestWhitespace, a.length)));
    }
}