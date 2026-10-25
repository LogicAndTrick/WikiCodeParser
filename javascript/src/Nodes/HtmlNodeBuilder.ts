import { HtmlHelper } from '../HtmlHelper';
import { HtmlNode } from './HtmlNode';
import { INode } from './INode';
import { NodeCollection } from './NodeCollection';

export class HtmlNodeBuilder {
    private readonly _attributes: Map<string, string>;
    private readonly _props: string[];
    private readonly _classes: Set<string>;
    private readonly _styles: Map<string, string>;

    public readonly tag: string;
    public get attributes(): ReadonlyMap<string, string> {
        return this._attributes;
    }
    public get props(): readonly string[] {
        return this._props;
    }
    public get classes(): ReadonlySet<string> {
        return this._classes;
    }
    public get styles(): ReadonlyMap<string, string> {
        return this._styles;
    }
    public contents: INode[];

    public static make(tag: string): HtmlNodeBuilder {
        return new HtmlNodeBuilder(tag);
    }

    constructor(tag: string) {
        if (!HtmlHelper.ValidateTagName(tag)) throw new Error('Invalid tag.');

        this.tag = tag;
        this._attributes = new Map<string, string>();
        this._props = [];
        this._classes = new Set<string>();
        this._styles = new Map<string, string>();
        this.contents = [];
    }

    public if(test: boolean, action: (builder: HtmlNodeBuilder) => void): HtmlNodeBuilder {
        if (test) action(this);
        return this;
    }

    public addClass(...classes: string[]): HtmlNodeBuilder {
        return this.addClasses(classes);
    }

    public addClasses(classes: Iterable<string>): HtmlNodeBuilder {
        for (const c of classes) this._classes.add(c);
        return this;
    }

    /** Styles are not sanitised, be sure to handle them with care! */
    public addStyle(prop: string, val: string): HtmlNodeBuilder {
        if (!HtmlHelper.ValidateStylePropertyName(prop)) throw new Error('Invalid property.');
        if (!HtmlHelper.ValidateStyleValue(val)) throw new Error('Invalid value.');
        this._styles.set(prop, val);
        return this;
    }

    public attr(key: string, val: string | null): HtmlNodeBuilder {
        if (!HtmlHelper.ValidateAttributeName(key)) throw new Error('Invalid attribute.');
        if ((key === 'href' || key === 'src') && !HtmlHelper.ValidateUrl(val ?? '')) throw new Error('Invalid url.');
        if (key === 'class') throw new Error('Use addClass instead.');
        if (key === 'style') throw new Error('Use addStyle instead.');
        this._attributes.set(key, val ?? '');
        return this;
    }

    public delAttr(key: string | null): HtmlNodeBuilder {
        if (key != null) this._attributes.delete(key);
        return this;
    }

    public prop(prop: string | null, on: boolean = true): HtmlNodeBuilder {
        if (!HtmlHelper.ValidateAttributeName(prop)) throw new Error('Invalid prop name.');
        if (prop === 'class') throw new Error('Use addClass instead.');
        if (prop === 'style') throw new Error('Use addStyle instead.');
        const index = this._props.indexOf(prop!);
        if (index >= 0) this._props.splice(index, 1);
        if (on) this._props.push(prop!);
        return this;
    }

    public add(node: INode): HtmlNodeBuilder {
        this.contents.push(node);
        return this;
    }

    public addRange(nodes: Iterable<INode> | null | undefined): HtmlNodeBuilder {
        if (nodes != null) this.contents.push(...nodes);
        return this;
    }

    public toNode({
        isBlockNode = false,
        plainBefore = '',
        plainAfter = '',
        selfClosing = false,
    }: {
        isBlockNode?: boolean;
        plainBefore?: string;
        plainAfter?: string;
        selfClosing?: boolean;
    } = {}): HtmlNode {
        if (this.contents.length > 0) selfClosing = false;

        const tag = HtmlHelper.ValidateTagName(this.tag) ? this.tag : 'div';
        const content: INode = this.contents.length === 1 ? this.contents[0] : new NodeCollection(...this.contents);
        let html = '<' + tag;
        if (this._classes.size > 0) {
            const classes = [...this._classes].sort().join(' ');
            html += ` class="${HtmlHelper.AttributeEncode(classes)}"`;
        }
        if (this._styles.size > 0) {
            html += ' style="';
            for (const prop of [...this._styles.keys()].sort()) {
                const value = this._styles.get(prop) ?? '';
                if (!HtmlHelper.ValidateStylePropertyName(prop)) continue;
                if (!HtmlHelper.ValidateStyleValue(value)) continue;
                html += HtmlHelper.AttributeEncode(prop + ':' + value + ';');
            }
            html += '"';
        }
        for (const attr of [...this._attributes.keys()].sort()) {
            if (attr === 'class' || attr === 'style') continue;
            if (!HtmlHelper.ValidateAttributeName(attr)) continue;
            let val = this._attributes.get(attr) ?? '';
            if (attr === 'href' || attr === 'src') {
                if (!HtmlHelper.ValidateUrl(val)) continue;
                val = HtmlHelper.SanitiseUrl(val);
            }
            val = HtmlHelper.AttributeEncode(val);
            html += ' ' + attr + '="' + val + '"';
        }
        for (const prop of [...this._props].sort()) {
            if (prop === 'class' || prop === 'style' || this._attributes.has(prop)) continue;
            if (!HtmlHelper.ValidateAttributeName(prop)) continue;
            html += ' ' + prop;
        }
        let after = '</' + tag + '>';
        if (selfClosing) {
            html += ' /';
            after = '';
        }
        html += '>';

        const node = new HtmlNode(html, content, after);
        node.IsBlockNode = isBlockNode;
        node.PlainBefore = plainBefore;
        node.PlainAfter = plainAfter;
        return node;
    }
}
