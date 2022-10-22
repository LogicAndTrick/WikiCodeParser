import { Parser } from '..';
import { HtmlNode } from '../Nodes/HtmlNode';
import { INode } from '../Nodes/INode';
import { PlainTextNode } from '../Nodes/PlainTextNode';
import { ParseData } from '../ParseData';
import { State } from '../State';
import { Tag } from './Tag';

export class YoutubeTag extends Tag {
    constructor() {
        super();
        this.Token = 'youtube';
        this.Element = 'div';
        this.MainOption = 'id';
        this.Options = ['id'];
    }

    public override FormatResult(parser: Parser, data: ParseData, state: State, scope: string, options: Record<string, string>, text: string): INode {
        let id = text;
        if (options['id']) id = options['id'];

        const classes = ['embedded', 'video'];
        if (this.ElementClass != null) classes.push(this.ElementClass);

        const captionNode = new HtmlNode('', PlainTextNode.Empty(), '');
        captionNode.PlainBefore = '[YouTube video] ';
        captionNode.PlainAfter = '\n';

        const before = `<div class="${classes.join(' ')}">` +
                     ' <div class="caption-panel">' +
                     '  <div class="video-container caption-body">' +
                     '   <div class="video-content">' +
                     `    <div class="uninitialised" data-youtube-id="${id}" style="background-image: url('https://i.ytimg.com/vi/${id}/hqdefault.jpg');"></div>` +
                     '   </div>' +
                     '  </div>';
        const after = '</div></div>';
        const ret = new HtmlNode(before, captionNode, after);
        ret.IsBlockNode = true;
        return ret;
    }

    public override Validate(options: Record<string, string>, text: string): boolean {
        let url = text;
        if (options['id']) url = options['id'];
        return url.match(/^[a-zA-Z0-9_-]{6,11}$/i) != null;
    }
}