import { HtmlHelper } from '../HtmlHelper';
import { HtmlNodeBuilder } from '../Nodes/HtmlNodeBuilder';
import { INode } from '../Nodes/INode';
import { MetadataNode } from '../Nodes/MetadataNode';
import { PlainTextNode } from '../Nodes/PlainTextNode';
import { UnprocessablePlainTextNode } from '../Nodes/UnprocessablePlainTextNode';
import { ParseData } from '../ParseData';
import { Parser } from '../Parser';
import { State } from '../State';
import { Template, Trim } from '../Util';
import { INodeProcessor } from './INodeProcessor';

export class MentionsProcessor implements INodeProcessor {
    public Priority = 12;

    public UrlFormatString: string;

    public constructor(urlFormatString: string) {
        this.UrlFormatString = urlFormatString;
    }

    ShouldProcess(node: INode, _scope: string): boolean {
        return node instanceof PlainTextNode && node.Text.includes('@');
    }

    Process(parser: Parser, data: ParseData, parentNode: INode | null, node: INode, _scope: string): INode[] {
        if (!(node instanceof PlainTextNode)) {
            return [node];
        }

        const ret: INode[] = [];
        const text = node.Text;

        /**
         * mention format = @<name>
         * name cannot contain newlines or the '>' character
         * name length must be between 1 and 64
         * name cannot start or end with whitespace
         */
        const state = new State(node.Line, node.Column, text);
        let plainPos = state.SourcePosition;
        let plain = '';
        while (!state.Done) {
            plain += state.ScanTo('@<');
            if (state.Done || state.Peek(2) != '@<') continue;

            const tagStart = state.SourcePosition;
            state.Seek(2, false); // @<

            const textStart = state.SourcePosition;
            const name = state.ScanTo('>');
            if (state.Done || name.indexOf('\n') >= 0 || state.Peek(1) != '>' || name.length < 1 || name.length > 64 || name.length != Trim(name).length) {
                plain += '@<' + name;
                continue;
            }

            state.Seek(1, false); // >

            if (plain.length > 0) ret.push(new PlainTextNode(plainPos, plain));

            ret.push(new MetadataNode(tagStart, 'Mention', name));
            ret.push(
                HtmlNodeBuilder.make('a', tagStart)
                    .addClass('mention')
                    .attr('data-name', name)
                    .attr('href', parser.Configuration.BuildUrl(Template(this.UrlFormatString, { 0: HtmlHelper.UrlEncode(name) })))
                    .add(new UnprocessablePlainTextNode(textStart, '@' + name))
                    .toNode()
            );

            plain = '';
            plainPos = state.SourcePosition;
        }

        plain += state.ScanToEnd();
        if (plain.length > 0) ret.push(new PlainTextNode(plainPos, plain));

        return ret;
    }
}
