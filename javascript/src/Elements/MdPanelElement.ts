import { Parser } from '..';
import { HtmlHelper } from '../HtmlHelper';
import { Lines } from '../Lines';
import { HtmlNode } from '../Nodes/HtmlNode';
import { INode } from '../Nodes/INode';
import { ParseData } from '../ParseData';
import { Element } from './Element';

export class MdPanelElement extends Element {
    public Matches(lines: Lines): boolean {
        return lines.Value().startsWith('~~~');
    }

    public Consume(parser: Parser, data: ParseData, lines: Lines, scope: string): INode {
        const current = lines.Current();

        const meta = lines.Value().substring(3).trim();
        let title = '';

        let found = false;
        const arr: string[] = [];
        while (lines.Next()) {
            const value = lines.Value().trimEnd();
            if (value == '~~~') {
                found = true;
                break;
            }

            if (value.length > 1 && value[0] == ':') title = value.substring(1).trim();
            else arr.push(value);
        }

        if (!found) {
            lines.SetCurrent(current);
            return null;
        }

        let cls: string;
        if (meta == 'message') cls = 'card-success';
        else if (meta == 'info') cls = 'card-info';
        else if (meta == 'warning') cls = 'card-warning';
        else if (meta == 'error') cls = 'card-danger';
        else cls = 'card-default';

        const before = `<div class="embed-panel card ${cls}">` +
            (title != '' ? `<div class="card-header">${HtmlHelper.Encode(title)}</div>` : '') +
            '<div class="card-body">';
        const content = parser.ParseElements(data, arr.join('\n'), scope);
        const after = '</div></div>';

        const node = new HtmlNode(before, content, after);
        node.PlainBefore = title == '' ? '' : title + '\n' + '-'.repeat(title.length) + '\n';
        return node;
    }
}
